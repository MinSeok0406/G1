using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// Host-authoritative Lobby Manager
    /// - Host만 데이터 원본을 보유/갱신
    /// - 마스터 승계 시 Ack 수집 후 복원 파이프라인 수행(타임아웃 내구성)
    /// - 생성된 LobbyPlayer의 ViewID를 사전에서 직접 관리(FindObjectsOfType 제거)
    /// </summary>
    public sealed class LobbyManager : SingletonNetworkBehaviour<LobbyManager>
    {
        // actorId -> LobbyPlayerData
        private readonly Dictionary<int, LobbyPlayerData> lobbyPlayers = new();

        // 생성된 로비 플레이어 ViewIDs (actorId -> viewId)
        private readonly Dictionary<int, int> actorToViewId = new();

        // 마스터 승계 Ack 수집
        private readonly HashSet<int> ackedActorIds = new();

        // ----- Settings -----
        [Header("Join/Retry")]
        [SerializeField] private float roomJoinTimeoutSeconds = 8f;
        [SerializeField] private float roomJoinRetryIntervalSeconds = 0.25f;

        [Header("Host Migration")]
        [SerializeField] private float ackTimeoutSeconds = 4f;     // 전원 Ack 대기 최대 시간
        [SerializeField] private int bulkResendLimit = 32;         // 재전송 시 최근 N개만(여기에선 1인 1개 데이터라 의미상 제한치)

        // 씬 키/이름은 Settings에서 가져가되, 존재하지 않을 경우 대비
        private string keyCurrentScene => Settings.currentSceneKey;
        private string sceneLobby => Settings.inGameLobbyScene;
        private string sceneMain  => Settings.testMainScene;

        private bool isHostInitialized = false; 

        protected override void Awake()
        {
            base.Awake();

            // 룸/프로퍼티 없을 수 있으므로 방어적으로 체크
            if (TryHandleSceneMismatchOrLeave())
                return;

            // 호스트만 룸 커스텀 씬 키 갱신
            if (PhotonNetwork.IsMasterClient)
            {
                HelperUtilities.SetCurrentScene(sceneLobby);
            }
        }

        private void Start()
        {
            CheckAndInitializeRoomJoin();
        }

        #region Room Join Init

        private void CheckAndInitializeRoomJoin() => StartCoroutine(WaitForJoinAndInitialize());

        private IEnumerator WaitForJoinAndInitialize()
        {
            var start = Time.time;
            while (Time.time - start < roomJoinTimeoutSeconds)
            {
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                {
                    InitializeAfterJoin();
                    yield break;
                }
                yield return new WaitForSeconds(roomJoinRetryIntervalSeconds);
            }
            HandleRoomJoinTimeout();
        }

        private bool TryHandleSceneMismatchOrLeave()
        {
            // InRoom + Room 존재 체크
            if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
                return false;

            if (PhotonNetwork.CurrentRoom.CustomProperties != null &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(keyCurrentScene, out object sceneObj))
            {
                string roomSceneName = sceneObj as string ?? sceneLobby;
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                if (!string.Equals(roomSceneName, currentScene, StringComparison.Ordinal))
                {
                    PhotonNetwork.IsMessageQueueRunning = false;
                    PhotonNetwork.LoadLevel(roomSceneName);
                    // 싱글톤 파괴는 씬 로드 이후에 발생하므로 별도 Destroy 불필요
                    return true;
                }
            }
            return false;
        }

        private void InitializeAfterJoin()
        {
            // 실제 앱에선 플랫폼 계정/구글 UID 등으로 대체
            string googleUID = System.Guid.NewGuid().ToString();
            int actorId = PhotonNetwork.LocalPlayer.ActorNumber;
            string nickname = $"Player{actorId}";

            var dynamicData = new LobbyPlayerDynamicData
            {
                posX = 0, posY = 0,
                isReady = false,
                playerCustomizationData = new PlayerCustomizationData { customColorId = 0, hatId = 0 }
            };

            PlayerManager.Instance.SetDynamicPlayerData(dynamicData);
            PlayerManager.Instance.SetStaticPlayerData(googleUID, nickname, actorId);

            string dynamicJson = JsonUtility.ToJson(dynamicData);

            // 등록 요청(호스트 권한으로 단일 소스 정리)
            photonView.RPC(nameof(Rpc_RequestRegisterLobbyPlayer),
                RpcTarget.MasterClient, googleUID, nickname, actorId, dynamicJson);
        }

        private void HandleRoomJoinTimeout()
        {
            Debug.LogWarning("[Lobby] Room join timeout. Returning to main.");
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel(sceneMain);
        }

        #endregion

        #region Register/Spawn (Host-only)

        [PunRPC]
        private void Rpc_RequestRegisterLobbyPlayer(string googleUID, string nickname, int actorId, string dynamicJson, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 요청 위조 방지: Sender 유효성
            if (info.Sender == null || info.Sender.ActorNumber != actorId)
            {
                Debug.LogWarning($"[Lobby] Register request spoof? sender={info.Sender?.ActorNumber} actorId={actorId}");
                return;
            }

            LobbyPlayerDynamicData dynamicData;
            try
            {
                dynamicData = JsonUtility.FromJson<LobbyPlayerDynamicData>(dynamicJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] DynamicData parse failed: {e}");
                return;
            }

            RegisterLobbyPlayer(googleUID, nickname, actorId, dynamicData);
            SpawnLobbyPlayer(actorId);

            // UI 반영(호스트 UI)
            LobbyUIManager.Instance?.RequestLobbySlotRefreshFromHost();
            LobbyUIManager.Instance?.ReadyCheckProcess();
        }

        public void RegisterLobbyPlayer(string googleUID, string nickname, int actorId, LobbyPlayerDynamicData dynamicData)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            lobbyPlayers[actorId] = new LobbyPlayerData
            {
                staticData = new LobbyPlayerStaticData { googleUID = googleUID, nickname = nickname, actorId = actorId, viewId = 0 },
                dynamicData = dynamicData
            };
        }

        public void SpawnLobbyPlayer(int actorId) => StartCoroutine(SpawnLobbyPlayerDelayed(actorId));

        private IEnumerator SpawnLobbyPlayerDelayed(int actorId)
        {
            yield return null; // 한 프레임 대기(씬/네트워크 오브젝트 안정화)

            if (!PhotonNetwork.IsMasterClient) yield break;
            if (!lobbyPlayers.TryGetValue(actorId, out var lp)) yield break;

            var prefab = GameResources.Instance?.lobbyPlayerPrefab;
            if (!prefab)
            {
                Debug.LogError("[Lobby] lobbyPlayerPrefab is missing.");
                yield break;
            }

            Vector3 pos = new Vector3(lp.dynamicData.posX, lp.dynamicData.posY, 0f);
            GameObject obj = PhotonNetwork.InstantiateRoomObject(prefab.name, pos, Quaternion.identity);

            if (!obj.TryGetComponent(out LobbyPlayer lobbyPlayer))
            {
                Debug.LogError("[Lobby] Spawned object has no LobbyPlayer component.");
                yield break;
            }

            int viewId = lobbyPlayer.photonView.ViewID;
            lp.staticData.viewId = viewId;
            lobbyPlayers[actorId] = lp;

            actorToViewId[actorId] = viewId;

            // 소유권 이전 + 초기화
            ReassignOwnershipToPlayer(viewId, actorId);
        }

        public void ReassignOwnershipToPlayer(int viewId, int newActorId)
        {
            var view = PhotonView.Find(viewId);
            if (!view || !PhotonNetwork.IsMasterClient) return;

            if (view.OwnerActorNr != newActorId)
            {
                TransferOwnershipSafe(view, newActorId);
            }

            // 소유자 확정 후 초기화 호출(호스트에서만)
            if (view.TryGetComponent(out LobbyPlayer player) && PhotonNetwork.IsMasterClient && !isHostInitialized)
            {
                player.InitializePlayer();
                isHostInitialized = true;
            }
        }

        public void TransferOwnershipSafe(PhotonView view, int newActorId)
        {
            if (!PhotonNetwork.IsMasterClient || !view) return;
            view.TransferOwnership(newActorId);
            Debug.Log($"[Ownership] View {view.ViewID} → Actor {newActorId}");
        }

        #endregion

        #region Player Leave / Removal

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            base.OnPlayerLeftRoom(otherPlayer);

            RemoveLobbyPlayer(otherPlayer.ActorNumber);

            LobbyUIManager.Instance?.RequestLobbySlotRefreshFromHost();
            LobbyUIManager.Instance?.ReadyCheckProcess();
        }

        public void RemoveLobbyPlayer(int actorId)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (!lobbyPlayers.ContainsKey(actorId))
                return;

            // View 제거
            if (actorToViewId.TryGetValue(actorId, out int viewId))
            {
                var view = PhotonView.Find(viewId);
                if (view)
                {
                    // RoomObject 이고 Owner가 0(룸)인 경우 소유자 위임 후 파괴
                    if (view.OwnerActorNr == 0)
                        view.TransferOwnership(PhotonNetwork.LocalPlayer);

                    if (view.IsMine || view.OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
                        PhotonNetwork.Destroy(view);
                    else
                        Debug.LogWarning($"[Lobby] Remove failed ViewID {viewId} owner={view.OwnerActorNr}");
                }
                actorToViewId.Remove(actorId);
            }

            // 데이터 제거
            lobbyPlayers.Remove(actorId);
        }

        #endregion

        #region Master Switch (Host Migration)

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);

            // 내가 새 호스트가 아니면 할 일 없음
            if (!PhotonNetwork.IsMasterClient)
                return;

            // Ack 수집 및 복원 파이프라인
            StartCoroutine(Co_HostMigrationRestore());
        }

        private IEnumerator Co_HostMigrationRestore()
        {
            // 1) 모든 클라에 캐시 요청 + Ack 대기
            yield return StartCoroutine(Co_RequestAllClientsToCacheDataWithAck());

            // 2) 기존 오브젝트 정리 후 재등록 요청
            RestoreAllLobbyPlayersFromClientCache();
        }

        private IEnumerator Co_RequestAllClientsToCacheDataWithAck()
        {
            ackedActorIds.Clear();

            // AllViaServer로 순서/일관성 확보
            photonView.RPC(nameof(Rpc_CacheMyDataAndSendAck), RpcTarget.AllViaServer);

            float start = Time.time;
            while (Time.time - start < ackTimeoutSeconds)
            {
                // 모든 현재 플레이어 Ack 수신하면 조기 종료
                if (ackedActorIds.Count >= PhotonNetwork.CurrentRoom.PlayerCount)
                    yield break;
                yield return null;
            }

            // 타임아웃: 부분 Ack 수신 상태로 진행(남은 인원은 이후 재등록 시 합류)
            if (ackedActorIds.Count < PhotonNetwork.CurrentRoom.PlayerCount)
            {
                Debug.LogWarning($"[HostMigration] Ack timeout: {ackedActorIds.Count}/{PhotonNetwork.CurrentRoom.PlayerCount}");
            }
        }

        [PunRPC]
        private void Rpc_CacheMyDataAndSendAck()
        {
            var my = PlayerManager.Instance?.GetMyLobbyPlayer();
            if (my)
            {
                PlayerManager.Instance.UpdateLobbyPlayerPos(my.transform.position);
            }

            int actorId = PhotonNetwork.LocalPlayer.ActorNumber;
            photonView.RPC(nameof(Rpc_ReceiveClientDataAck), RpcTarget.MasterClient, actorId);
        }

        [PunRPC]
        private void Rpc_ReceiveClientDataAck(int actorId)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            ackedActorIds.Add(actorId);
        }

        private void RestoreAllLobbyPlayersFromClientCache()
        {
            // 현재 트래킹된 오브젝트만 정확히 제거(씬 전역 탐색 금지)
            DestroyAllLobbyPlayersTracked();

            // 각 클라에 재등록 요청
            photonView.RPC(nameof(Rpc_RequestClientsToResendData), RpcTarget.AllViaServer);
        }

        [PunRPC]
        private void Rpc_RequestClientsToResendData()
        {
            var data = PlayerManager.Instance.GetMergedPlayerData();
            string dynamicJson = JsonUtility.ToJson(data.dynamicData);

            photonView.RPC(nameof(Rpc_RequestRegisterLobbyPlayer), RpcTarget.MasterClient,
                data.staticData.googleUID, data.staticData.nickname, data.staticData.actorId, dynamicJson);
        }

        private void DestroyAllLobbyPlayersTracked()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int destroyed = 0;
            // actorToViewId를 사용해 정확히 파괴
            foreach (var kv in actorToViewId.ToList())
            {
                var view = PhotonView.Find(kv.Value);
                if (!view) { actorToViewId.Remove(kv.Key); continue; }

                if (view.OwnerActorNr == 0)
                    view.TransferOwnership(PhotonNetwork.LocalPlayer);

                if (view.IsMine || view.OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    PhotonNetwork.Destroy(view);
                    destroyed++;
                }
                else
                {
                    Debug.LogWarning($"[HostMigration] Failed to destroy ViewID {view.ViewID}, Owner: {view.OwnerActorNr}");
                }
                actorToViewId.Remove(kv.Key);
            }
            Debug.Log($"[HostMigration] Destroyed {destroyed} LobbyPlayers (tracked)");
        }

        #endregion

        #region Public APIs

        public List<LobbyPlayerData> GetLobbyPlayerList() => lobbyPlayers.Values.ToList();

        public void UpdateReadyState(int actorId, bool isReady)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (!lobbyPlayers.TryGetValue(actorId, out var lp)) return;
            lp.dynamicData.isReady = isReady;
            lobbyPlayers[actorId] = lp;
            LobbyUIManager.Instance?.RequestLobbySlotRefreshFromHost();
        }

        #endregion
    }
}

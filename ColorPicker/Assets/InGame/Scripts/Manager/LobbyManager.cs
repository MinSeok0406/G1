using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class LobbyManager : SingletonNetworkBehaviour<LobbyManager>
    {
        private Dictionary<int, LobbyPlayerData> lobbyPlayers = new Dictionary<int, LobbyPlayerData>();
        HashSet<int> ackedActorIds = new HashSet<int>();

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            CheckAndInitializeRoomJoin();
        }

        /// <summary>
        /// 룸 참가 초기화 코루틴 실행하는 함수
        /// </summary>
        private void CheckAndInitializeRoomJoin()
        {
            StartCoroutine(VerifyRoomJoinAndSetup());
        }

        /// <summary>
        /// 일정 시간 동안 InRoom 여부 확인 후 초기화 또는 타임아웃 처리하는 함수
        /// </summary>
        private IEnumerator VerifyRoomJoinAndSetup()
        {
            float connectStartTime = Time.time;

            while (Time.time < connectStartTime + Settings.RoomJoinTimeoutSeconds)
            {
                if (PhotonNetwork.InRoom)
                {
                    InitializeAfterJoin();
                    yield break;
                }
                yield return new WaitForSeconds(Settings.RoomJoinRetryIntervalSeconds);
            }

            HandleRoomJoinTimeout();
        }

        /// <summary>
        /// 로컬 플레이어의 LobbyPlayer 데이터를 초기화하고 MasterClient에게 등록 요청하는 함수
        /// </summary>
        private void InitializeAfterJoin()
        {
            string googleUID = System.Guid.NewGuid().ToString(); // TODO: Replace with actual ID fetch
            int actorId = PhotonNetwork.LocalPlayer.ActorNumber;
            string nickname = $"Player{actorId}";

            var dynamicData = new LobbyPlayerDynamicData
            {
                posX = 0,
                posY = 0,
                isReady = false,
                playerCustomizationData = new PlayerCustomizationData { customColorId = 0, hatId = 0 }
            };

            PlayerManager.Instance.SetDynamicPlayerData(dynamicData);
            PlayerManager.Instance.SetStaticPlayerData(googleUID, nickname, actorId);

            string dynamicJson = JsonUtility.ToJson(dynamicData);
            photonView.RPC(nameof(Rpc_RequestRegisterLobbyPlayer), RpcTarget.MasterClient, googleUID, nickname, actorId, dynamicJson);
        }

        /// <summary>
        /// 룸 참가 실패 시 로비로 이동 처리하는 함수
        /// </summary>
        private void HandleRoomJoinTimeout()
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel(Settings.testMainScene);
        }

        /// <summary>
        /// 클라이언트에서 보낸 등록 요청을 마스터가 처리하고 LobbyPlayer를 생성하는 RPC 함수
        /// </summary>
        /// <param name="googleUID">플레이어 고유 ID</param>
        /// <param name="nickname">닉네임</param>
        /// <param name="actorId">Photon Actor 번호</param>
        /// <param name="dynamicJson">직렬화된 동적 데이터</param>
        [PunRPC]
        private void Rpc_RequestRegisterLobbyPlayer(string googleUID, string nickname, int actorId, string dynamicJson)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var dynamicData = JsonUtility.FromJson<LobbyPlayerDynamicData>(dynamicJson);
            RegisterLobbyPlayer(googleUID, nickname, actorId, dynamicData);
            SpawnLobbyPlayer(actorId);
        }

        /// <summary>
        /// 마스터가 서버 내 딕셔너리에 로비 플레이어 데이터를 등록하는 함수
        /// </summary>
        /// <param name="googleUID">플레이어 고유 ID</param>
        /// <param name="nickname">닉네임</param>
        /// <param name="actorId">Photon Actor 번호</param>
        /// <param name="dynamicData">동적 데이터 객체</param>
        public void RegisterLobbyPlayer(string googleUID, string nickname, int actorId, LobbyPlayerDynamicData dynamicData)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            lobbyPlayers[actorId] = new LobbyPlayerData
            {
                staticData = new LobbyPlayerStaticData { googleUID = googleUID, nickname = nickname, actorId = actorId },
                dynamicData = dynamicData
            };

            LobbyUIManager.Instance.RequestLobbySlotRefreshFromHost();
        }

        /// <summary>
        /// 마스터가 LobbyPlayer RoomObject를 생성하고 소유권 이전 처리하는 함수
        /// </summary>
        /// <param name="actorId">Photon Actor 번호</param>
        public void SpawnLobbyPlayer(int actorId)
        {
            if (!lobbyPlayers.ContainsKey(actorId)) return;

            string prefabName = GameResources.Instance.lobbyPlayerPrefab.name;
            Vector2 pos = new(lobbyPlayers[actorId].dynamicData.posX, lobbyPlayers[actorId].dynamicData.posY);
            GameObject playerObj = PhotonNetwork.InstantiateRoomObject(prefabName, pos, Quaternion.identity);

            if (playerObj.TryGetComponent(out LobbyPlayer player))
            {
                int viewId = player.photonView.ViewID;
                lobbyPlayers[actorId].staticData.viewId = viewId;
                ReassignOwnershipToPlayer(viewId, actorId);
            }
        }

        /// <summary>
        /// 지정된 ViewId의 객체 소유권을 해당 actor에게 넘기고 초기화 호출하는 함수
        /// </summary>
        /// <param name="viewId">PhotonView의 ID</param>
        /// <param name="newActorId">새로운 소유자 Actor ID</param>
        public void ReassignOwnershipToPlayer(int viewId, int newActorId)
        {
            if (PhotonView.Find(viewId)?.TryGetComponent(out LobbyPlayer player) != true) return;

            if (player.photonView.OwnerActorNr != newActorId)
            {
                TransferOwnershipSafe(player.photonView, newActorId);
                Debug.Log($"[Ownership] Transferred to Actor {newActorId}");
            }

            if (player.photonView.OwnerActorNr == newActorId && PhotonNetwork.IsMasterClient)
            {
                player.InitializedPlayer();
            }
        }

        /// <summary>
        /// 안전하게 PhotonView의 소유권을 이전하는 함수
        /// </summary>
        /// <param name="view">PhotonView 객체</param>
        /// <param name="newActorId">새로운 소유자 Actor ID</param>
        public void TransferOwnershipSafe(PhotonView view, int newActorId)
        {
            if (!PhotonNetwork.IsMasterClient || view == null) return;

            view.TransferOwnership(newActorId);
            Debug.Log($"[Ownership] Ownership transferred to Actor {newActorId}");
        }

        /// <summary>
        /// 플레이어 퇴장 시 해당 LobbyPlayer를 제거하는 함수
        /// </summary>
        /// <param name="otherPlayer">Photon 퇴장한 플레이어</param>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            base.OnLeftRoom();

            RemoveLobbyPlayer(otherPlayer.ActorNumber);

            LobbyUIManager.Instance.RequestLobbySlotRefreshFromHost();
        }

        /// <summary>
        /// actorId에 해당하는 LobbyPlayer 오브젝트 및 데이터를 제거하는 함수
        /// </summary>
        /// <param name="actorId">삭제할 Actor ID</param>
        public void RemoveLobbyPlayer(int actorId)
        {
            if (!PhotonNetwork.IsMasterClient || !lobbyPlayers.ContainsKey(actorId)) return;

            int viewId = lobbyPlayers[actorId].staticData.viewId;
            PhotonView view = PhotonView.Find(viewId);

            if (view != null)
            {
                PhotonNetwork.Destroy(view);
            }

            lobbyPlayers.Remove(actorId);
        }

        /// <summary>
        /// 마스터 교체 시 소유권 이전 및 복원 흐름을 시작하는 함수
        /// </summary>
        /// <param name="newMasterClient">새로운 마스터 클라이언트</param>
        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);
            if (!PhotonNetwork.IsMasterClient) return;

            foreach (var view in FindObjectsOfType<PhotonView>())
            {
                if (view.IsRoomView() && view.OwnerActorNr == 0)
                {
                    view.TransferOwnership(PhotonNetwork.LocalPlayer);
                }
            }

            RequestAllClientsToCacheDataWithAck();
        }

        /// <summary>
        /// 모든 클라이언트에 데이터 캐싱 요청을 보내고 Ack를 기다리는 함수
        /// </summary>
        private void RequestAllClientsToCacheDataWithAck()
        {
            ackedActorIds.Clear();
            photonView.RPC(nameof(Rpc_CacheMyDataAndSendAck), RpcTarget.All);
        }

        /// <summary>
        /// 클라이언트가 자신의 데이터를 캐싱하고 마스터에게 Ack를 보내는 RPC 함수
        /// </summary>
        [PunRPC]
        private void Rpc_CacheMyDataAndSendAck()
        {
            var myPlayer = PlayerManager.Instance.GetMyLobbyPlayer();
            PlayerManager.Instance.UpdateLobbyPlayerPos(myPlayer.transform.position);

            int actorId = PhotonNetwork.LocalPlayer.ActorNumber;
            photonView.RPC(nameof(Rpc_ReceiveClientDataAck), RpcTarget.MasterClient, actorId);
        }

        /// <summary>
        /// 마스터가 Ack를 수신하고, 전체 수신 시 복원 시작하는 RPC 함수
        /// </summary>
        /// <param name="actorId">Ack를 보낸 클라이언트의 Actor ID</param>
        [PunRPC]
        private void Rpc_ReceiveClientDataAck(int actorId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            ackedActorIds.Add(actorId);
            if (ackedActorIds.Count >= PhotonNetwork.CurrentRoom.PlayerCount)
            {
                RestoreAllLobbyPlayersFromClientCache();
            }
        }

        /// <summary>
        /// 기존 LobbyPlayer 오브젝트를 삭제하고 클라이언트에 재등록 요청을 보내는 함수
        /// </summary>
        private void RestoreAllLobbyPlayersFromClientCache()
        {
            DestroyAllLobbyPlayers();
            photonView.RPC(nameof(Rpc_RequestClientsToResendData), RpcTarget.All);
        }

        /// <summary>
        /// 각 클라이언트에게 자신의 LobbyPlayer 데이터를 다시 Master에게 전송하도록 요청하는 RPC 함수
        /// </summary>
        [PunRPC]
        private void Rpc_RequestClientsToResendData()
        {
            var data = PlayerManager.Instance.GetMergedPlayerData();
            string dynamicJson = JsonUtility.ToJson(data.dynamicData);

            photonView.RPC(nameof(Rpc_RequestRegisterLobbyPlayer), RpcTarget.MasterClient,
                data.staticData.googleUID, data.staticData.nickname, data.staticData.actorId, dynamicJson);
        }

        /// <summary>
        /// 마스터가 모든 LobbyPlayer RoomObject를 삭제하는 함수
        /// </summary>
        public void DestroyAllLobbyPlayers()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int count = 0;
            foreach (var player in FindObjectsOfType<LobbyPlayer>())
            {
                PhotonView view = player.photonView;

                if (view != null && view.IsRoomView())
                {
                    if (view.OwnerActorNr == 0)
                    {
                        view.TransferOwnership(PhotonNetwork.LocalPlayer);
                    }

                    if (view.IsMine)
                    {
                        PhotonNetwork.Destroy(view);
                        count++;
                    }
                    else
                    {
                        Debug.LogWarning($"[HostMigration] Failed to destroy ViewID {view.ViewID}, Owner: {view.Owner?.ActorNumber}");
                    }
                }
            }

            Debug.Log($"[HostMigration] Destroyed {count} LobbyPlayers");
        }

        /// <summary>
        /// 로비 플레이어 데이터를 리스트 형태로 반환하는 함수
        /// </summary>
        /// <returns></returns>
        public List<LobbyPlayerData> GetLobbyPlayerList()
        {
            return lobbyPlayers.Values.ToList();
        }

        public void UpdateReadyState(int actorId, bool isReady)
        {
            lobbyPlayers[actorId].dynamicData.isReady = isReady;
        }
    }
}

using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public sealed class LobbyUIManager : SingletonNetworkBehaviour<LobbyUIManager>
    {
        [Header("UI Elements")]
        [SerializeField] private Button readyButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button interactButton; // 상호작용 버튼

        [SerializeField] private GameObject playerSlotsRoot;
        [SerializeField] private Transform playerContainer;
        [SerializeField] private GameObject playerSlotPrefab;

        [SerializeField] private TMP_Text playerCountText;
        [SerializeField] private TMP_Text readyButtonLabel;   // "Ready/Cancel" 텍스트 연결

        private readonly List<PlayerSlotUI> slotPool = new();
        private readonly Dictionary<int, PlayerSlotUI> actorIdToSlotUIMap = new();

        // Host only: actorId -> isReady
        private readonly Dictionary<int, bool> isReadyMap = new();

        private bool isLocalReady;
        private bool isStartingGame;

        // 상호작용 버튼 기본 클릭 동작
        private UnityEngine.Events.UnityAction _defaultInteractionClick;

        #region Unity

        protected override void Awake()
        {
            base.Awake();

            // 널 가드
            if (!playerContainer)
            {
                Debug.LogError("[LobbyUI] playerContainer is not assigned.");
                return;
            }
            if (!playerSlotPrefab)
            {
                Debug.LogError("[LobbyUI] playerSlotPrefab is not assigned.");
                return;
            }

            InitSlotPool();

        }

        private void Start()
        {
            if (readyButton)
            {
                readyButton.onClick.RemoveAllListeners();
                readyButton.onClick.AddListener(OnReadyButtonClicked);
            }
            if (startButton)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(OnClickStartGame);
            }

            // 상호작용 버튼 초기화
            InitializeInteractionButton();

            UpdatePlayerCountUI();
        }

        private void OnDestroy()
        {
            if (readyButton) readyButton.onClick.RemoveAllListeners();
            if (startButton) startButton.onClick.RemoveAllListeners();
            if (interactButton) interactButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 상호작용 버튼 초기화
        /// </summary>
        private void InitializeInteractionButton()
        {
            _defaultInteractionClick = () => { /* 기본 동작 없음 */ };

            if (interactButton != null)
            {
                interactButton.onClick.RemoveAllListeners();
                interactButton.onClick.AddListener(_defaultInteractionClick);
                interactButton.gameObject.SetActive(false); // 기본적으로 숨김
            }
        }

        #endregion

        #region Slot Pool / Refresh

        private void InitSlotPool()
        {
            slotPool.Clear();
            actorIdToSlotUIMap.Clear();

            for (int i = 0; i < Settings.maxPlayer; i++)
            {
                var go = Instantiate(playerSlotPrefab, playerContainer);
                var slot = go.GetComponent<PlayerSlotUI>();
                if (!slot)
                {
                    Debug.LogError("[LobbyUI] PlayerSlotUI missing on prefab.");
                    continue;
                }
                go.SetActive(false);
                slotPool.Add(slot);
            }
        }

        private void ClearAllSlots()
        {
            foreach (var slot in slotPool) slot.gameObject.SetActive(false);
            actorIdToSlotUIMap.Clear();
        }

        private void RefreshPlayerSlotList(List<LobbyPlayerData> lobbyPlayerDatas)
        {
            ClearAllSlots();

            int idx = 0;
            foreach (var data in lobbyPlayerDatas)
            {
                if (idx >= slotPool.Count)
                {
                    Debug.LogWarning("[LobbyUI] Slot pool exhausted.");
                    break;
                }

                var slot = slotPool[idx];
                slot.InitializedSlot(data);
                slot.gameObject.SetActive(true);

                actorIdToSlotUIMap[data.staticData.actorId] = slot;

                // 이미 알고 있는 Ready 상태 있으면 즉시 반영
                if (PhotonNetwork.IsMasterClient &&
                    isReadyMap.TryGetValue(data.staticData.actorId, out var isR))
                {
                    slot.SetReady(isR);
                }

                idx++;
            }

            UpdatePlayerCountUI();
        }

        public void SetPlayerSlotsActive(bool isActive)
        {
            if (playerSlotsRoot) playerSlotsRoot.SetActive(isActive);
        }

        private void UpdatePlayerCountUI()
        {
            var room = PhotonNetwork.CurrentRoom;
            if (room == null || !playerCountText) return;

            playerCountText.text = $"{room.PlayerCount} / {room.MaxPlayers}";
        }


        #endregion

        #region Host: Slot/Ready Sync

        /// <summary>호스트: 현재 로비 플레이어 리스트를 직렬화하여 배포</summary>
        public void RequestLobbySlotRefreshFromHost()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 슬롯 리스트 브로드캐스트
            OnLobbyPlayerSlotUpdateRequested();

            // 레디맵을 최신 플레이어 목록으로 동기화하고 브로드캐스트
            SyncIsReadyMapWithCurrentPlayers();
            BroadcastReadyMapToAll();
        }

        private string SerializeLobbyPlayerList()
        {
            if (!PhotonNetwork.IsMasterClient) return string.Empty;

            var wrapper = new LobbyPlayerDataListWrapper
            {
                lobbyPlayerDatas = LobbyManager.Instance.GetLobbyPlayerList()
            };
            return JsonUtility.ToJson(wrapper);
        }

        private void OnLobbyPlayerSlotUpdateRequested()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            string json = SerializeLobbyPlayerList();
            photonView.RPC(nameof(Rpc_ReceiveUpdatedLobbyPlayerSlot), RpcTarget.AllViaServer, json);
        }

        [PunRPC]
        private void Rpc_ReceiveUpdatedLobbyPlayerSlot(string json)
        {
            var wrapper = JsonUtility.FromJson<LobbyPlayerDataListWrapper>(json);
            RefreshPlayerSlotList(wrapper.lobbyPlayerDatas);
        }

        /// <summary>호스트: 현재 룸 구성에 따라 isReadyMap 정리</summary>
        private void SyncIsReadyMapWithCurrentPlayers()
        {
            if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null) return;

            var currentActorIds = PhotonNetwork.CurrentRoom.Players.Values
                .Select(p => p.ActorNumber).ToHashSet();

            // 제거
            var toRemove = isReadyMap.Keys.Where(id => !currentActorIds.Contains(id)).ToList();
            foreach (var id in toRemove) isReadyMap.Remove(id);

            // 추가(기본 false)
            foreach (var actorId in currentActorIds)
            {
                if (!isReadyMap.ContainsKey(actorId))
                    isReadyMap[actorId] = false;
            }
        }

        /// <summary>호스트: 레디맵 전체를 클라에 일괄 반영(신규 참여자 초기화용)</summary>
        private void BroadcastReadyMapToAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (isReadyMap.Count == 0) return;

            var actorIds = isReadyMap.Keys.ToArray();
            var states = actorIds.Select(id => isReadyMap[id]).ToArray();
            photonView.RPC(nameof(Rpc_BulkReadyStateUpdate), RpcTarget.AllViaServer, actorIds, states);
        }

        [PunRPC]
        private void Rpc_BulkReadyStateUpdate(int[] actorIds, bool[] states)
        {
            if (actorIds == null || states == null) return;
            int n = Math.Min(actorIds.Length, states.Length);

            for (int i = 0; i < n; i++)
            {
                if (actorIdToSlotUIMap.TryGetValue(actorIds[i], out var slot))
                    slot.SetReady(states[i]);
            }

        }

        #endregion

        #region Ready Button & Flow

        private void OnReadyButtonClicked()
        {
            isLocalReady = !isLocalReady;
            PlayerManager.Instance.UpdateReadyState(isLocalReady);

            var me = PhotonNetwork.LocalPlayer;
            if (me == null) return;

            // 버튼 라벨 즉시 반영(클라 UX)
            if (readyButtonLabel)
                readyButtonLabel.text = isLocalReady ? "Cancel" : "Ready";

            photonView.RPC(nameof(Rpc_RequestReadyUpdate), RpcTarget.MasterClient, me.ActorNumber, isLocalReady);
        }

        [PunRPC]
        private void Rpc_RequestReadyUpdate(int actorId, bool isReady, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 보낸 주체 검증
            if (info.Sender == null || info.Sender.ActorNumber != actorId)
            {
                Debug.LogWarning($"[LobbyUI] Spoofed ready request? sender={info.Sender?.ActorNumber}, actorId={actorId}");
                return;
            }

            var room = PhotonNetwork.CurrentRoom;
            if (room == null || !room.Players.ContainsKey(actorId))
            {
                Debug.LogWarning($"[LobbyUI] Ready request rejected: actorId {actorId} not in room.");
                return;
            }

            // 레디맵 업데이트
            isReadyMap[actorId] = isReady;
            LobbyManager.Instance.UpdateReadyState(actorId, isReady);

            // 단건 반영
            photonView.RPC(nameof(Rpc_ReceivePlayerReadyState), RpcTarget.AllViaServer, actorId, isReady);

            // 전체 레디 여부 체크
            ReadyCheckProcess();
        }

        public void ReadyCheckProcess()
        {
            // 호스트만 판단/시작 버튼 토글
            if (!PhotonNetwork.IsMasterClient) return;

            bool allReady = AreAllPlayersReady();
            if (allReady)
            {
                startButton.gameObject.SetActive(true);
                startButton.interactable = allReady && !isStartingGame;
            }
            else
            {
                startButton.gameObject.SetActive(false);
            }
        }

        [PunRPC]
        private void Rpc_ReceivePlayerReadyState(int actorId, bool isReady)
        {
            if (actorIdToSlotUIMap.TryGetValue(actorId, out var slot))
                slot.SetReady(isReady);

        }

        private bool AreAllPlayersReady()
        {
            var room = PhotonNetwork.CurrentRoom;
            if (room == null) return false;

            foreach (var kv in room.Players)
            {
                int actorId = kv.Key;
                if (!isReadyMap.TryGetValue(actorId, out bool ready) || !ready)
                    return false;
            }
            return room.PlayerCount > 0; // 최소 1명 이상일 때만 true
        }

        #endregion

        #region Interaction Button

        /// <summary>
        /// 상호작용 버튼 표시/숨김 및 이벤트 설정
        /// </summary>
        public void ShowInteractionButton(bool show, UnityEngine.Events.UnityAction onClick = null)
        {
            if (interactButton == null) return;

            if (show)
            {
                interactButton.onClick.RemoveAllListeners();
                interactButton.onClick.AddListener(onClick ?? _defaultInteractionClick);
                interactButton.gameObject.SetActive(true);
            }
            else
            {
                interactButton.onClick.RemoveAllListeners();
                interactButton.onClick.AddListener(_defaultInteractionClick);
                interactButton.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Start Game

        public void OnClickStartGame()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (isStartingGame) return;           // 중복 가드
            if (!AreAllPlayersReady()) return;    // 안전망

            isStartingGame = true;

            // 커스터마이즈 데이터를 LobbyPlayerData에 저장
            var lobbyPlayerDataList = LobbyManager.Instance.GetLobbyPlayerList();
            PopulateCustomizationData(lobbyPlayerDataList);

            CacheDataManager.Instance.SaveAll(lobbyPlayerDataList);

            photonView.RPC(nameof(Rpc_StartGame), RpcTarget.AllViaServer);
        }

        /// <summary>
        /// LobbyPlayerData에 CustomizeManager의 색상 데이터를 저장
        /// </summary>
        private void PopulateCustomizationData(List<LobbyPlayerData> lobbyPlayerDataList)
        {
            if (CustomizeManager.Instance == null)
            {
                Debug.LogWarning("[LobbyUIManager] CustomizeManager not found while populating customization data");
                return;
            }

            foreach (var playerData in lobbyPlayerDataList)
            {
                if (playerData == null || playerData.staticData == null) continue;

                int actorId = playerData.staticData.actorId;
                int colorIndex = CustomizeManager.Instance.GetPlayerColorIndex(actorId);

                if (colorIndex >= 0)
                {
                    // dynamicData가 null이면 초기화
                    if (playerData.dynamicData == null)
                    {
                        playerData.dynamicData = new LobbyPlayerDynamicData();
                    }

                    // playerCustomizationData가 null이면 초기화
                    if (playerData.dynamicData.playerCustomizationData == null)
                    {
                        playerData.dynamicData.playerCustomizationData = new PlayerCustomizationData();
                    }

                    playerData.dynamicData.playerCustomizationData.customColorId = colorIndex;
                    Debug.Log($"[LobbyUIManager] Saved color {colorIndex} for actor {actorId} to cache");
                }
                else
                {
                    Debug.LogWarning($"[LobbyUIManager] No color found for actor {actorId}");
                }
            }
        }

        [PunRPC]
        public void Rpc_StartGame()
        {
            // 씬 동기화 키 업데이트 + 로컬 로드
            HelperUtilities.SetCurrentScene(Settings.inGameScene);
            SceneManager.LoadScene(Settings.inGameScene);
        }

        #endregion
    }
}

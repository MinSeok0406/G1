using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class LobbyUIManager : SingletonNetworkBehaviour<LobbyUIManager>
    {
        [Header("UI Elements")]
        [SerializeField] private Button readyButton;
        [SerializeField] private Button startButton;
        
        [SerializeField] private GameObject playerSlots;
        [SerializeField] private GameObject playerContainer;
        [SerializeField] private GameObject playerSlotPrefab;

        [SerializeField] private TMP_Text playerCountText;

        private List<PlayerSlotUI> slotPool = new List<PlayerSlotUI>();
        private Dictionary<int, PlayerSlotUI> actorIdToSlotUIMap = new Dictionary<int, PlayerSlotUI>();
        private Dictionary<int, bool> isReadyMap = new Dictionary<int, bool>(); // [호스트 전용]

        private bool isLocalReady = false;

        protected override void Awake()
        {
            base.Awake();

            slotPool.Clear();

            InitSlotPool();
        }

        private void Start()
        {
            readyButton.onClick.AddListener(OnReadyButtonClicked);

            startButton.onClick.AddListener(OnClickStartGame);
        }

        public void InitSlotPool()
        {
            for (int i = 0; i < Settings.maxPlayer; i++)
            {
                GameObject obj = Instantiate(playerSlotPrefab, playerContainer.transform);
                var slot = obj.GetComponent<PlayerSlotUI>();
                slotPool.Add(slot);

                slot.gameObject.SetActive(false);
            }
        }

        public void RequestLobbySlotRefreshFromHost()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            OnLobbyPlayerSlotUpdateRequested();
            SyncIsReadyMapWithCurrentPlayers();
        }

        public string SerializeLobbyPlayerList()
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

            photonView.RPC(nameof(Rpc_ReceiveUpdatedLobbyPlayerSlot), RpcTarget.All, json);
        }

        [PunRPC]
        private void Rpc_ReceiveUpdatedLobbyPlayerSlot(string json)
        {
            LobbyPlayerDataListWrapper wrapper = JsonUtility.FromJson<LobbyPlayerDataListWrapper>(json);
            RefreshPlayerSlotList(wrapper.lobbyPlayerDatas);
        }

        private void RefreshPlayerSlotList(List<LobbyPlayerData> lobbyPlayerDatas)
        {
            ClearAllSlots();

            int index = 0;

            foreach(LobbyPlayerData data in lobbyPlayerDatas)
            {
                if (index >= slotPool.Count){ Debug.Log($"[LobbyUIManager] 슬롯 개수 초과"); break;}

                slotPool[index].InitializedSlot(data);
                slotPool[index].gameObject.SetActive(true);
                actorIdToSlotUIMap.Add(data.staticData.actorId, slotPool[index]);
                index++;
            }

            UpdatePlayerCountUI();
        }

        private void ClearAllSlots()
        {
            foreach (var slot in slotPool)
            {
                slot.gameObject.SetActive(false);
            }

            actorIdToSlotUIMap.Clear();

        }

        public void SetPlayerSlotsActive(bool isActive)
        {
            playerSlots.SetActive(isActive);
        }

        private void UpdatePlayerCountUI()
        {
            if (PhotonNetwork.CurrentRoom == null) return;

            int current = PhotonNetwork.CurrentRoom.PlayerCount;
            int max = PhotonNetwork.CurrentRoom.MaxPlayers;

            playerCountText.text = $"{current} / {max}";
        }

        private void SyncIsReadyMapWithCurrentPlayers()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var currentActorIds = PhotonNetwork.CurrentRoom.Players.Values
                .Select(p => p.ActorNumber).ToHashSet();

            var keysToRemove = isReadyMap.Keys.Where(id => !currentActorIds.Contains(id)).ToList();
            foreach (var id in keysToRemove)
            {
                isReadyMap.Remove(id);
                Debug.Log($"[Lobby] isReadyMap에서 나간 플레이어 {id} 제거됨");
            }

            foreach (var actorId in currentActorIds)
            {
                if (!isReadyMap.ContainsKey(actorId))
                {
                    isReadyMap[actorId] = false;
                    Debug.Log($"[Lobby] isReadyMap에 새 플레이어 {actorId} 추가됨");
                }
            }
        }

        private void OnReadyButtonClicked()
        {
            isLocalReady = !isLocalReady;

            PlayerManager.Instance.UpdateReadyState(isLocalReady);

            int actorId = PhotonNetwork.LocalPlayer.ActorNumber;

            photonView.RPC(nameof(Rpc_RequestReadyUpdate), RpcTarget.MasterClient, actorId, isLocalReady);
        }

        [PunRPC]
        private void Rpc_RequestReadyUpdate(int actorId, bool isReady)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (!PhotonNetwork.CurrentRoom.Players.ContainsKey(actorId))
            {
                Debug.LogWarning($"[Lobby] 준비 요청 무시됨: actorId {actorId} 는 현재 룸에 존재하지 않습니다.");
                return;
            }

            if (!isReadyMap.ContainsKey(actorId))
            {
                Debug.LogWarning($"[Lobby] isReadyMap에 없는 actorId {actorId} → false로 초기화 후 상태 반영");
                isReadyMap[actorId] = false;
            }

            isReadyMap[actorId] = isReady;

            LobbyManager.Instance.UpdateReadyState(actorId, isReady);

            photonView.RPC(nameof(Rpc_ReceivePlayerReadyState), RpcTarget.All, actorId, isReady);

            if (AreAllPlayersReady())
            {
                Debug.Log("[Lobby] 전체 플레이어 준비 완료!");
                // StartGame() 또는 StartButton 활성화 등 처리
                startButton.gameObject.SetActive(true);
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
            {
                slot.SetReady(isReady);
                Debug.Log($"[LobbyUI] actor {actorId} → Ready 상태 업데이트: {isReady}");
            }
            else
            {
                Debug.LogWarning($"[LobbyUI] actorId {actorId}에 해당하는 슬롯이 없습니다.");
            }
        }

        private bool AreAllPlayersReady()
        {
            var currentActorIds = PhotonNetwork.CurrentRoom.Players.Keys;

            foreach (int actorId in currentActorIds)
            {
                if (!isReadyMap.TryGetValue(actorId, out bool isReady) || !isReady)
                {
                    return false;
                }
            }

            return true;
        }

        public void OnClickStartGame()
        {
            if (!PhotonNetwork.IsMasterClient || !AreAllPlayersReady()) return;

            photonView.RPC(nameof(Rpc_StartGame), RpcTarget.All);
        }

        [PunRPC]
        public void Rpc_StartGame()
        {
            SceneManager.LoadScene(Settings.InGameScene);
        }
    }
}

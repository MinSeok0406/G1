using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace ColorPicker.InGame
{
    public class NetworkManager : SingletonNetworkBehaviour<NetworkManager>
    {
        [HideInInspector] public PlayerData currentPlayerData;
       
        public Player MyPlayer { get; private set; }

        private Dictionary<int, PlayerData> playerDictionary = new Dictionary<int, PlayerData>();
        private Dictionary<int, Player> playerObjectDictionary = new Dictionary<int, Player>();

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SpawnPlayer();
        }

        // 플레이어를 서버에 등록/스폰하는 함수
        public void SpawnPlayer()
        {
            string prefabName = GameResources.Instance.playerPrefab.name;

            // 클라이언트가 서버에 접속시 자신 소유의 캐릭터를 서버에 등록
            Player player = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity).GetComponent<Player>();

            if (player.photonView.IsMine)
            {
                // 자신 소유의 캐릭터를 참조하기 쉽도록 등록
                MyPlayer = player;
            }
        }

        // 플레이어 오브젝트를 등록 (for 플레이어 데이터 정보를 최신화)
        public void RegisterPlayer(int playerId, Player player)
        {
            if (!playerObjectDictionary.ContainsKey(playerId))
                playerObjectDictionary.Add(playerId, player);

            if (!PhotonNetwork.IsMasterClient) return;

            // 새로운 플레이어가 방에 입장할 경우 방의 모든 플레이어의 데이터 갱신
            SyncPlayerData();

        }

        public void UnregisterPlayerObject(int playerId)
        {
            if (playerObjectDictionary.ContainsKey(playerId))
                playerObjectDictionary.Remove(playerId);
        }

        public void SyncPlayerData()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            List<PlayerData> clone = new List<PlayerData>(playerDictionary.Values);

            foreach (PlayerData player in clone)
            {
                PacketHandler.Instance.SendPlayerData(player);
            }
        }

        // 만약 플레이어가 딕셔너리에 없다면 플레이어를 추가 이미 있다면 데이터를 업데이트하는 함수
        public void AddOrUpdatePlayerData(PlayerData playerData)
        {
            if (!playerDictionary.ContainsKey(playerData.playerId))
            {
                playerDictionary.Add(playerData.playerId, playerData);
            }
            else
            {
                playerDictionary[playerData.playerId] = playerData;

            }

            playerObjectDictionary[playerData.playerId].playerDataChangedEvent.CallPlayerDataChanageEvent(playerData);
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            // 플레이어가 방을 떠나면 딕셔너리에서 해당 플레이어 데이터를 제거
            RemovePlayerData(otherPlayer);
        }

        // 딕셔너리에서 playerId를 사용하여 플레이어 데이터를 제거
        public void RemovePlayerData(Photon.Realtime.Player player)
        {
            int playerId = player.ActorNumber;

            if (playerDictionary.ContainsKey(playerId))
            {
                playerDictionary.Remove(playerId);
            }
        }

        public Dictionary<int, PlayerData> GetPlayerDictionary()
        {
            return playerDictionary;
        }

        // out 키워드로 다른 클래스에서 호출하고 변경할 수 있도록 설정
        public bool TryGetPlayerData(int playerId, out PlayerData data)
        {
            return playerDictionary.TryGetValue(playerId, out data);
        }

        public Player GetPlayerObject(int playerId)
        {
            playerObjectDictionary.TryGetValue(playerId, out var player);
            return player;
        }
    }
}

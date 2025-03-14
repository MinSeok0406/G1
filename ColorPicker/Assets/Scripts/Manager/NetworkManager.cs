using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace ColorPicker.InGame
{
    public class NetworkManager : SingletonNetworkBehaviour<NetworkManager>
    {
        private Dictionary<int, Player> playerDictionary = new Dictionary<int, Player>();
 
        public Player MyPlayer { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            S_SpawnPlayer();
        }

        public void S_SpawnPlayer()
        {
            string prefabName = GameResources.Instance.playerPrefab.name;

            Player player = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity).GetComponent<Player>();

            if (player.photonView.IsMine)
            {
                MyPlayer = player;   
            }
        }

        public void S_RegisterPlayer(int playerId, Player player)
        {
            photonView.RPC("S_RegisterPlayerToServer", RpcTarget.MasterClient, playerId, player);
        }

        public void S_UnRegisterPlayer(int playerId)
        {
            photonView.RPC("S_UnRegisterPlayerFromServer", RpcTarget.MasterClient, playerId);
        }

        public Dictionary<int, Player> S_GetPlayerDictionary()
        {
            return playerDictionary;
        }

        [PunRPC]
        public void S_RegisterPlayerToServer(int playerId, Player player)
        {
            if (!playerDictionary.ContainsKey(playerId))
            {
                playerDictionary.Add(playerId, player);
            }
        }

        [PunRPC]
        public void S_UnRegisterPlayerFromServer(int playerId)
        {
            if (playerDictionary.ContainsKey(playerId))
            {
                Destroy(playerDictionary[playerId].gameObject);

                playerDictionary.Remove(playerId);
            }
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            Destroy(gameObject);
        }
    }
}

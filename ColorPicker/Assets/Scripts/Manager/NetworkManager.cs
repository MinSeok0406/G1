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

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            S_SpawnPlayer();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach(int playerid in playerDictionary.Keys)
                {
                    Debug.Log(playerid);
                }
            }
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

        public void S_RegisterPlayer(int playerId, PlayerData playerData)
        {
            photonView.RPC("S_RegisterPlayerToServer", RpcTarget.MasterClient, playerId, playerData);
        }

        public void S_UnRegisterPlayer(int playerId)
        {
            photonView.RPC("S_UnRegisterPlayerFromServer", RpcTarget.MasterClient, playerId);
        }

        public void C_RegisterPlayer(int playerId, PlayerData playerData)
        {
            Photon.Realtime.Player targetPlayer = PhotonNetwork.LocalPlayer;

            photonView.RPC("C_RegisterPlayerToClient", targetPlayer, playerId, playerData);
        }

        public void C_UnRegisterPlayer(int playerId)
        {
            Photon.Realtime.Player targetPlayer = PhotonNetwork.LocalPlayer;

            photonView.RPC("C_UnRegisterPlayerFromClient", targetPlayer, playerId);
        }

        public Dictionary<int, PlayerData> GetPlayerDictionary()
        {
            return playerDictionary;
        }

        [PunRPC]
        public void S_RegisterPlayerToServer(int playerId, PlayerData playerData)
        {
            if (!playerDictionary.ContainsKey(playerId))
            {
                playerDictionary.Add(playerId, playerData);
            }
        }

        [PunRPC]
        public void S_UnRegisterPlayerFromServer(int playerId)
        {
            if (playerDictionary.ContainsKey(playerId))
            {
                Destroy(playerDictionary[playerId].player.gameObject);

                playerDictionary.Remove(playerId);
            }
        }

        [PunRPC]
        public void C_RegisterPlayerToClient(int playerId, PlayerData playerData)
        {
            if (!playerDictionary.ContainsKey(playerId))
            {
                playerDictionary.Add(playerId, playerData);
            }
        }

        [PunRPC]
        public void C_UnRegisterPlayerFromClient(int playerId)
        {
            if (playerDictionary.ContainsKey(playerId))
            {
                Destroy(playerDictionary[playerId].player.gameObject);

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

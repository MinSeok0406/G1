using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace ColorPicker.InGame
{
    public class NetworkManager : SingletonNetworkBehaviour<NetworkManager>
    {
        private Dictionary<int, Player> playerDictionary = new Dictionary<int, Player>();
        public List<Player> players = new List<Player>();  // test

        private int currentPlayerId;
        public Player MyPlayer { get; private set;}

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        public override void OnLeftRoom()
        {
            base.OnLeftLobby();

            if (PhotonNetwork.IsMasterClient)
            {
                int playerId = PhotonNetwork.LocalPlayer.ActorNumber;

                UnRegisterPlayer(playerId);
            }

            Destroy(gameObject);
        }

        public void SetMyPlayer(Player player)
        {
            MyPlayer = player;
        }

        public void RegisterClient(int playerId)
        {
            photonView.RPC("RegisterClientToServer", RpcTarget.MasterClient, playerId);
        }

        public void RegisterPlayer(Player player)
        {
            photonView.RPC("RegisterPlayerToServer", RpcTarget.MasterClient, currentPlayerId, player);
        }

        public void UnRegisterPlayer(int playerId)
        {
            photonView.RPC("UnRegisterPlayerFromServer", RpcTarget.MasterClient, playerId);
        }

        public Dictionary<int, Player> GetPlayerDictionary()
        {
            return playerDictionary;
        }

        [PunRPC]
        public void RegisterClientToServer(int playerId)
        {
            if (!playerDictionary.ContainsKey(playerId))
            {
                playerDictionary.Add(playerId, null);

                currentPlayerId = playerId;
            }
        }

        [PunRPC]
        public void RegisterPlayerToServer(int playerId, Player player)
        {
            if (!playerDictionary.ContainsKey(playerId))
            {
                playerDictionary.Add(playerId, player);

                players.Add(player); // test
            }

            Debug.Log(playerId);
        }

        [PunRPC]
        public void UnRegisterPlayerFromServer(int playerId)
        {
            if (playerDictionary.ContainsKey(playerId))
            {
                Destroy(playerDictionary[playerId].gameObject);

                playerDictionary.Remove(playerId);
            }
        }


    }
}

using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class LobbyManager : SingletonNetworkBehaviour<LobbyManager>, IPunObservable
    {
        private Dictionary<int, bool> readyPlayerList = new Dictionary<int, bool>();

        public event Action OnReadyEvent;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            C_RegisterPlayerToReadyList();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            int playerId = otherPlayer.ActorNumber;

            photonView.RPC("S_UnRegisterPlayerFromReadyList", RpcTarget.MasterClient, playerId);
        }

        public void C_SetReady()
        {
            int playerId = PhotonNetwork.LocalPlayer.ActorNumber;

            photonView.RPC("S_UpdatePlayerReadyStatus", RpcTarget.MasterClient, playerId);

            OnReadyEvent?.Invoke();
        }

        public int S_CheckPlayersReady()
        {
            int readyPlayerCount = 0;

            foreach (KeyValuePair<int, bool> player in readyPlayerList)
            {
                if (player.Value)
                {
                    readyPlayerCount++;
                }
            }

            return readyPlayerCount;
        }


        private void C_RegisterPlayerToReadyList()
        {
            int playerId = PhotonNetwork.LocalPlayer.ActorNumber;

            photonView.RPC("S_RegisterPlayerToReadyList", RpcTarget.MasterClient, playerId);
        }

        private void C_UnRegisterPlayerFromReadyList()
        {
            int playerId = PhotonNetwork.LocalPlayer.ActorNumber;

            photonView.RPC("S_UnRegisterPlayerFromReadyList", RpcTarget.MasterClient, playerId);
        }

        
        //인원제한 추후 추가 필요
        public void H_GameStart()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            photonView.RPC("C_StartGameOnClients", RpcTarget.All);
        }
      
        [PunRPC]
        private void C_StartGameOnClients()
        {
            SceneManager.LoadScene("InGame");
        }


        [PunRPC]
        private void S_UpdatePlayerReadyStatus(int playerID)
        {
            if (readyPlayerList.ContainsKey(playerID))
            {
                readyPlayerList[playerID] = !readyPlayerList[playerID];
            }
            else
            {
                readyPlayerList.Add(playerID, false);
            }
        }

        [PunRPC]
        private void S_RegisterPlayerToReadyList(int playerID)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (!readyPlayerList.ContainsKey(playerID))
            {
                readyPlayerList.Add(playerID, false);
            }
        }

        [PunRPC]
        private void S_UnRegisterPlayerFromReadyList(int playerID)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (readyPlayerList.ContainsKey(playerID))
            {
                readyPlayerList.Remove(playerID);

            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {

        }
    }
}

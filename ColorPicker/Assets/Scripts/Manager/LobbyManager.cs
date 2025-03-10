using Photon.Pun;
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
        public GameObject playerPrefab;
        public Button startButton;
        public Button readyButton;
        public TMP_Text playerStateText;

        private Dictionary<int, bool> readyPlayerList = new Dictionary<int, bool>();

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);

            int playerId = PhotonNetwork.LocalPlayer.ActorNumber;

            photonView.RPC("RegisterPlayer", RpcTarget.MasterClient, playerId);

            if (PhotonNetwork.IsMasterClient)
            {
                startButton.gameObject.SetActive(true);
                startButton.interactable = false;

                UpdatePlayerStateUI();
            }


        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            UpdatePlayerStateUI();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            int playerId = PhotonNetwork.LocalPlayer.ActorNumber;
            readyPlayerList.Remove(playerId);

            SceneManager.LoadScene("MainMenu");
        }

        public void SetReady()
        {
            StartCoroutine(SetReadyProcess());   
        }

        public IEnumerator SetReadyProcess()
        {
            int playerID = PhotonNetwork.LocalPlayer.ActorNumber;

            photonView.RPC("UpdatePlayerReadyStatus", RpcTarget.MasterClient, playerID);

            readyButton.interactable = false;

            yield return new WaitForSeconds(1f);

            readyButton.interactable = true;
        }

        private int CheckPlayersReady()
        {
            int readyPlayerCount = 0;

            foreach (var player in readyPlayerList)
            {
                if (player.Value)
                {
                    readyPlayerCount++;
                }
            }
            return readyPlayerCount;
        }

        public void GameStart()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            photonView.RPC("StartGameOnClients", RpcTarget.All);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {

            }
            else
            {

            }
        }

        private void UpdatePlayerStateUI()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("UpdatePlayerReadyStateUI", RpcTarget.All, CheckPlayersReady(), PhotonNetwork.CurrentRoom.PlayerCount, PhotonNetwork.CurrentRoom.MaxPlayers);
            }
        }

        #region for Clients
        [PunRPC]
        private void StartGameOnClients()
        {
            SceneManager.LoadScene("InGame");
        }

        [PunRPC]
        public void UpdatePlayerReadyStateUI(int readyPlayer, int playerAmount, int maxPlayer)
        {
            playerStateText.text = $"Player : {readyPlayer} / {playerAmount} (MaxPlayer:{maxPlayer})";
        }
        #endregion

        #region for Server
        [PunRPC]
        private void UpdatePlayerReadyStatus(int playerID)
        {
            if (readyPlayerList.ContainsKey(playerID))
            {
                readyPlayerList[playerID] = !readyPlayerList[playerID];
            }
            else
            {
                readyPlayerList.Add(playerID, false);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                if (CheckPlayersReady() == readyPlayerList.Count)
                {
                    startButton.interactable = true;
                }
                else
                {
                    startButton.interactable = false;
                }
            }

            UpdatePlayerStateUI();
        }


        [PunRPC]
        private void RegisterPlayer(int playerID)
        {
            if (!readyPlayerList.ContainsKey(playerID))
            {
                readyPlayerList.Add(playerID, false);
            }
        }
        #endregion
    }
}

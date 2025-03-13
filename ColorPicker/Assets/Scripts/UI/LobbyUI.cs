
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class LobbyUI : MonoBehaviourPunCallbacks
    {
        public Button startButton;
        public Button readyButton;
        public TMP_Text playerStateText;

        private void Start()
        {
            S_UpdatePlayerStateUI();

            H_SetStartButton();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            S_UpdatePlayerStateUI();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            LobbyManager.Instance.OnReadyEvent += LobbyManager_OnReadyEvent;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            LobbyManager.Instance.OnReadyEvent -= LobbyManager_OnReadyEvent;
        }

        private void LobbyManager_OnReadyEvent()
        {
            photonView.RPC("S_UpdatePlayerStateUI", RpcTarget.MasterClient);

            StartCoroutine(C_SetReadyButtonEvent());

            H_InteractableStartButton();
        }

        [PunRPC]
        private void S_UpdatePlayerStateUI()
        {
            photonView.RPC("C_UpdatePlayerReadyStateUI", RpcTarget.All,
                LobbyManager.Instance.S_CheckPlayersReady(), PhotonNetwork.CurrentRoom.PlayerCount, PhotonNetwork.CurrentRoom.MaxPlayers);
        }

        private IEnumerator C_SetReadyButtonEvent()
        {
            int playerID = PhotonNetwork.LocalPlayer.ActorNumber;

            readyButton.interactable = false;

            yield return new WaitForSeconds(1f);

            readyButton.interactable = true;
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);

            S_UpdatePlayerStateUI();
        }


        private void H_SetStartButton()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                startButton.gameObject.SetActive(true);
                startButton.interactable = false;
            }
        }

        private void H_InteractableStartButton()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (LobbyManager.Instance.S_CheckPlayersReady() == PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    startButton.interactable = true;
                }
                else
                {
                    startButton.interactable = false;
                }
            }
        }

        [PunRPC]
        public void C_UpdatePlayerReadyStateUI(int readyPlayer, int playerAmount, int maxPlayer)
        {
            playerStateText.text = $"Player : {readyPlayer} / {playerAmount} (MaxPlayer:{maxPlayer})";
        }
    }
}

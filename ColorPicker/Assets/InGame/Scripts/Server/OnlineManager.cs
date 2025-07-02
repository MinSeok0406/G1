using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

namespace ColorPicker.InGame
{
    public class OnlineManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_Text connectionText;

        private const string roomName = "MainLobby";
        private void Start()
        {
            // 테스트 코드
            PhotonNetwork.OfflineMode = false;
            PhotonNetwork.GameVersion = "1.0.0";
            PhotonNetwork.PhotonServerSettings.DevRegion = "kr";
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";
            // 테스트 코드

            PhotonNetwork.ConnectUsingSettings();

            if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
            {
                Debug.Log("a");
                PhotonNetwork.ConnectUsingSettings();
            }

            joinButton.interactable = false;
            connectionText.text = "Connecting...";
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Debug.Log($"[PHOTON] Connected to Master. Region: {PhotonNetwork.CloudRegion}");
            joinButton.interactable = true;
            connectionText.text = "Connected";
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);

            joinButton.interactable = false;
            connectionText.text = "Offline: Reconnecting...";

            if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }

            Debug.LogWarning($"[PHOTON] Disconnected. Cause: {cause}");

            PhotonNetwork.ConnectUsingSettings();
        }

        public void Connect()
        {
            joinButton.interactable = false;

            if (PhotonNetwork.IsConnected)
            {
                connectionText.text = "Joining room...";
                PhotonNetwork.JoinRoom(roomName);
            }
            else
            {
                connectionText.text = "Offline: Reconnecting...";
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            connectionText.text = "Room not found. Creating...";

            RoomOptions options = new RoomOptions
            {
                MaxPlayers = 10,
                IsOpen = true,
                IsVisible = true
            };

            PhotonNetwork.CreateRoom(roomName, options);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            PhotonNetwork.LoadLevel("InLobby");
        }

    }
}

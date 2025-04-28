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

        private void Start()
        {
            PhotonNetwork.GameVersion = Settings.gameVersion;
            PhotonNetwork.ConnectUsingSettings();

            joinButton.interactable = false;
            connectionText.text = "접속 중...."; 
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            joinButton.interactable = true;
            connectionText.text = "연결됨";
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);

            joinButton.interactable = false;
            connectionText.text = "오프라인 : 접속 재시도 중...";

            PhotonNetwork.ConnectUsingSettings();
        }

        public void Connect()
        {
            joinButton.interactable = false;

            if (PhotonNetwork.IsConnected)
            {
                connectionText.text = "방에 입장....";
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                connectionText.text = "오프라인 : 접속 재시도 중...";

                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);

            connectionText.text = "빈 방 없음, 새로운 방 생성";

            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 10});
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            PhotonNetwork.LoadLevel("Lobby");
        }

    }
}

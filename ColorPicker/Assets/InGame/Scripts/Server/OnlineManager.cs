using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

namespace ColorPicker.InGame
{
    // 마스터 서버와 룸 접속 담당
    public class OnlineManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_Text connectionText;

        // 게임 실행과 동시에 마스터 서버 접속 시도
        private void Start()
        {
            PhotonNetwork.GameVersion = Settings.gameVersion;
            PhotonNetwork.ConnectUsingSettings();

            joinButton.interactable = false;
            connectionText.text = "접속 중...."; 
        }

        // 마스터 서버 접속 성공 시 자동 실행
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            joinButton.interactable = true;
            connectionText.text = "연결됨";
        }

        // 마스터 서버 접속 실패 시 자동 실행
        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);

            joinButton.interactable = false;
            connectionText.text = "오프라인 : 접속 재시도 중...";

            PhotonNetwork.ConnectUsingSettings();
        }

        // 룸 접속 시도
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

        // 빈 방이 없어 랜덤 룸 참가에 실패한 경우 자동 실행
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            base.OnJoinRandomFailed(returnCode, message);

            connectionText.text = "빈 방 없음, 새로운 방 생성";

            // 리슨 서버
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 10});
        }

        // 룸에 참가 완료된 경우 자동 실행
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            PhotonNetwork.LoadLevel("InLobby");
        }

    }
}

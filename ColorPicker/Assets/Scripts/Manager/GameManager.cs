using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorPicker.InGame
{
    public class GameManager : SingletonNetworkBehaviour<GameManager>, IPunObservable
    {
        public GameObject playerPrefab;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PhotonNetwork.LeaveRoom();
            }
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

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            SceneManager.LoadScene("MainMenu");
        }
    }
}

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ColorPicker.InGame
{
    public class SetFollowLighting : MonoBehaviourPunCallbacks
    {
        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            SetFollowLight();
        }

        private void SetFollowLight()
        {
           Dictionary<int,Player> players = NetworkManager.Instance.GetPlayerDictionary();

            foreach(int playerId in players.Keys)
            {
                Photon.Realtime.Player targetPlayer = PhotonNetwork.CurrentRoom.Players[playerId];

                //????????없으면 오류
                Debug.Log(playerId); Debug.Log(targetPlayer == null);

                photonView.RPC("SetFollowLightToClients", targetPlayer);
            }
        }

        [PunRPC]
        private void SetFollowLightToClients()
        {
            transform.SetParent(NetworkManager.Instance.MyPlayer.transform);
            transform.localPosition = Vector3.zero;
        }
    }
}

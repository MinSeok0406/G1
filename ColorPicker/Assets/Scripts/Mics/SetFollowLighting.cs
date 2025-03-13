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
            S_SetFollowLight();
        }

        private void S_SetFollowLight()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Dictionary<int,Player> players = NetworkManager.Instance.S_GetPlayerDictionary();

            foreach(int playerId in players.Keys)
            {
                Photon.Realtime.Player targetPlayer = PhotonNetwork.CurrentRoom.Players[playerId];

                photonView.RPC("C_SetFollowLightToClients", targetPlayer);
            }
        }

        [PunRPC]
        private void C_SetFollowLightToClients()
        {
            transform.SetParent(NetworkManager.Instance.MyPlayer.transform);
            transform.localPosition = Vector3.zero;
        }
    }
}

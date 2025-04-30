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
            transform.SetParent(NetworkManager.Instance.MyPlayer.transform);
            transform.localPosition = Vector3.zero;
        }

    }
}

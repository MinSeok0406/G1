using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class CameraSetup : MonoBehaviourPun
    {
        private void Start()
        {
            if (photonView.IsMine)
            {
                Camera mainCamera = Camera.main;

                mainCamera.transform.SetParent(transform);
                mainCamera.transform.localPosition = new Vector3(0, 0, -10);
            }

        }
    }
}

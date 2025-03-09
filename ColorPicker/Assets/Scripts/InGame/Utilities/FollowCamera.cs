using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class FollowCamera : NetworkBehaviour
    {
        private void Start()
        {
            if (isOwned)
            {
                Camera mainCamera = Camera.main;
                mainCamera.transform.SetParent(transform);
                transform.localPosition = new Vector3(0, 0, Settings.cameraLocalPositionZ);
                mainCamera.orthographicSize = Settings.cameraOrthographicSize;
            }
        }
    }
}

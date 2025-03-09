using ColorPicker.InGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.Server
{
    public class OnlineUI : MonoBehaviour
    {
        public void OnClickCreateRoom()
        {
            var manager = RoomManager.singleton;

            manager.StartHost();
        }

        public void OnClickEnterGameRoom()
        {
            var manager = RoomManager.singleton;

            manager.StartClient();
        }
        
    }
}

using ColorPicker.InGame;
using Mirror;
using UnityEngine;

namespace ColorPicker.Server
{
    public class RoomManager : NetworkRoomManager
    {
        public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnRoomServerAddPlayer(conn);

            var player = Instantiate(RoomManager.singleton.spawnPrefabs[0]);
            NetworkServer.Spawn(player, conn);
        }
    }
}


using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorPicker.InGame
{
    public class RoomManager : SingletonNetworkBehaviour<RoomManager>
    {
        private Dictionary<int, CustomizationColor> playerColorMap = new();
        private List<CustomizationColor> allColors;

        protected override void Awake()
        {
            base.Awake();

            allColors = Enum.GetValues(typeof(CustomizationColor)).Cast<CustomizationColor>().ToList();
        }

        public void RequestColorChange(int playerId, CustomizationColor newColor)
        {
            if (!PhotonNetwork.IsMasterClient) return; // Host에서만 동작

            if (playerColorMap.ContainsValue(newColor)) return; // 이미 사용 중인 색이면 무시

            playerColorMap[playerId] = newColor;

            photonView.RPC("SyncColorToAll", RpcTarget.All, playerId, (int)newColor);
        }

        public void RequestColorChange(int playerId, int newColorIndex)
        {
            photonView.RPC("HandleColorChangeRequest", RpcTarget.MasterClient, playerId, newColorIndex);
        }

        [PunRPC]
        public void HandleColorChangeRequest(int playerId, int newColorIndex)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (NetworkManager.Instance.TryGetPlayerData(playerId, out PlayerData playerData))
            {
                playerData.playerColor = newColorIndex;
            }

            NetworkManager.Instance.SyncPlayerData();
        }

        public List<CustomizationColor> GetAvailableColors()
        {
            return allColors.Except(playerColorMap.Values).ToList();
        }
    }
}

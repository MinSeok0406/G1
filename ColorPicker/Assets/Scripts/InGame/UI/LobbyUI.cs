using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPicker.InGame
{
    public class LobbyUI : MonoBehaviour
    {
        public Button startButton;

        [Server]
        public void OnClickReady()
        {
            List<Player> players = GameSystem.Instance.GetPlayerList();
            foreach(Player player in players)
            {
                if (player.isLocalPlayer)
                {
                    GameSystem.Instance.AddReadyPlayer(player);
                    break;
                }
            }

            if (NetworkServer.active && NetworkClient.localPlayer != null)
            {
                if (GameSystem.Instance.CheckAllPlayerReady())
                {
                    startButton.gameObject.SetActive(true);
                }
                else
                {
                    startButton.gameObject.SetActive(false);
                }
            }

        }

        [Server]
        public void OnClickStart()
        {
            GameSystem.Instance.GameStart();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class InteractiveObject : MonoBehaviour
    {
        private MyPlayerControl MyPlayer;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            MyPlayer = collision.gameObject.GetComponent<MyPlayerControl>();

            if (MyPlayer != null)
            {
                if (gameObject.tag == "Custom")
                {
                    UI_BackGround gameSceneUI = Managers.UI.SceneUI as UI_BackGround;
                    UI_LobbyScene lobbyUI = gameSceneUI.mLobbyUI;

                    lobbyUI.GetImage((int)UI_LobbyScene.Images.Custom_mButton).gameObject.SetActive(true);
                }
                else if (gameObject.tag == "InGame")
                {
                    UI_BackGround gameSceneUI = Managers.UI.SceneUI as UI_BackGround;
                    UI_LobbyScene lobbyUI = gameSceneUI.mLobbyUI;

                    lobbyUI.GetImage((int)UI_LobbyScene.Images.InGame_mButton).gameObject.SetActive(true);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (MyPlayer != null)
            {
                if (gameObject.tag == "Custom")
                {
                    UI_BackGround gameSceneUI = Managers.UI.SceneUI as UI_BackGround;
                    UI_LobbyScene lobbyUI = gameSceneUI.mLobbyUI;

                    lobbyUI.GetImage((int)UI_LobbyScene.Images.Custom_mButton).gameObject.SetActive(false);
                }
                else if (gameObject.tag == "InGame")
                {
                    UI_BackGround gameSceneUI = Managers.UI.SceneUI as UI_BackGround;
                    UI_LobbyScene lobbyUI = gameSceneUI.mLobbyUI;

                    lobbyUI.GetImage((int)UI_LobbyScene.Images.InGame_mButton).gameObject.SetActive(false);
                }
            }
        }
    }
}
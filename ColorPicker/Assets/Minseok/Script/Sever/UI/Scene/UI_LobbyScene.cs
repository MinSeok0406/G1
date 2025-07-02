using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minseok
{
    public class UI_LobbyScene : UI_Base
    {
        public enum Images
        {
            Chat_mButton,
            Custom_mButton,
            InGame_mButton,
        }

        public override void Init()
        {
            Bind<Image>(typeof(Images));

            GetImage((int)Images.Chat_mButton).gameObject.BindEvent(OpenChat);
            GetImage((int)Images.Custom_mButton).gameObject.BindEvent(OpenCustom);
            GetImage((int)Images.InGame_mButton).gameObject.BindEvent(OpenInGame);

            GetImage((int)Images.Custom_mButton).gameObject.SetActive(false);
            GetImage((int)Images.InGame_mButton).gameObject.SetActive(false);
        }

        public void OpenChat(PointerEventData evt)
        {
            UI_BackGround backSceneUI = Managers.UI.SceneUI as UI_BackGround;
            UI_ChatScene chatUI = backSceneUI.ChatUI;

            if (!chatUI.gameObject.activeSelf)
            {
                chatUI.gameObject.SetActive(true);
                Managers.UI.ClosePopupUI();
                gameObject.SetActive(false);
            }
        }

        public void OpenCustom(PointerEventData evt)
        {
            UI_BackGround backSceneUI = Managers.UI.SceneUI as UI_BackGround;
            UI_CustomScene customUI = backSceneUI.CustomUI;

            if (!customUI.gameObject.activeSelf)
            {
                customUI.gameObject.SetActive(true);
                Managers.UI.ClosePopupUI();
                gameObject.SetActive(false);
            }
        }

        public void OpenInGame(PointerEventData evt)
        {
            UI_BackGround backSceneUI = Managers.UI.SceneUI as UI_BackGround;
            UI_InGameScene ingameUI = backSceneUI.InGameUI;

            if (!ingameUI.gameObject.activeSelf)
            {
                ingameUI.gameObject.SetActive(true);
                Managers.UI.ClosePopupUI();
                gameObject.SetActive(false);
            }
        }
    }
}
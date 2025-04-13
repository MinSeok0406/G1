using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobbyScene : UI_Base
{
    enum Images
    {
        Chat_mButton
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));

        GetImage((int)Images.Chat_mButton).gameObject.BindEvent(OpenChat);
    }

    public void OpenChat(PointerEventData evt)
    {
        UI_BackGround gameSceneUI = Managers.UI.SceneUI as UI_BackGround;
        UI_ChatScene chatUI = gameSceneUI.ChatUI;

        if (chatUI.gameObject.activeSelf)
        {
            chatUI.gameObject.SetActive(false);
            gameObject.SetActive(true);
            //GetImage((int)Images.Chat_mButton).gameObject.SetActive(true);
        }
        else
        {
            chatUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
            //GetImage((int)Images.Chat_mButton).gameObject.SetActive(false);
        }
    }
}

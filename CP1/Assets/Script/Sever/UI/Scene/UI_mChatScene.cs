using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_mChatScene : UI_Base
{
    bool _active = false;

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
        _active = !_active;

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        UI_ChatScene chatUI = gameSceneUI.ChatUI;

        if (_active)
        {
            chatUI.gameObject.SetActive(true);
        }
        else
        {
            chatUI.gameObject.SetActive(false);
        }
    }
}

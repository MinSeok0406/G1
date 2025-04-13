using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BackGround : UI_Scene
{
    public UI_LobbyScene mLobbyUI { get; private set; }
    public UI_ChatScene ChatUI { get; private set; }

    public override void Init()
    {
        base.Init();

        mLobbyUI = GetComponentInChildren<UI_LobbyScene>();
        ChatUI = GetComponentInChildren<UI_ChatScene>();

        mLobbyUI.gameObject.SetActive(true);
        ChatUI.gameObject.SetActive(false);
    }
}

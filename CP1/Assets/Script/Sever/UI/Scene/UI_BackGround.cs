using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BackGround : UI_Scene
{
    public UI_mChatScene mChatUI { get; private set; }

    public override void Init()
    {
        base.Init();

        mChatUI = GetComponentInChildren<UI_mChatScene>();

        mChatUI.gameObject.SetActive(false);
    }
}

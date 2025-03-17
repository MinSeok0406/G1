using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameScene : UI_Scene
{
    public UI_ChatScene ChatUI { get; private set; }

    public override void Init()
    {
        base.Init();

        ChatUI = GetComponentInChildren<UI_ChatScene>();

        ChatUI.gameObject.SetActive(false);
    }
}

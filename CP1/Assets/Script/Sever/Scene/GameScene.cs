using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    UI_GameScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        Managers.Scene.StartScene();

        Screen.SetResolution(640, 480, false);

        Application.runInBackground = true;

        _sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>("UI");
    }

    public void OpenChat()
    {
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        UI_ChatScene chatUI = gameSceneUI.ChatUI;

        if (chatUI.gameObject.activeSelf)
        {
            chatUI.gameObject.SetActive(false);
        }
        else
        {
            chatUI.gameObject.SetActive(true);
        }
    }

    public override void Clear()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    UI_BackGround _backScene;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Lobby;

        Screen.SetResolution(640, 480, false);

        Application.runInBackground = true;

        _backScene = Managers.UI.ShowSceneUI<UI_BackGround>("UI_BackGround");
    }

    public override void Clear()
    {
        
    }
}

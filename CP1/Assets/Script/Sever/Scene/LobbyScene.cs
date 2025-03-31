using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Lobby;

        Screen.SetResolution(640, 480, false);

        Application.runInBackground = true;
    }

    public override void Clear()
    {
        
    }
}

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

    /*private void Update()
    {
        if (Input.GetKeyUp(KeyCode.K))
        {
            foreach (GameObject obj in Managers.Object._objects.Values)
            {
                PlayerControl cc = obj.GetComponent<PlayerControl>();
                Debug.Log($"{cc.Id}, {cc.PosInfo}");
            }
        }

    }*/

    public override void Clear()
    {
        
    }
}

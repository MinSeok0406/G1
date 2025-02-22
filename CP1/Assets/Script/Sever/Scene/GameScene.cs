using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        Managers.Scene.StartScene();

        Screen.SetResolution(640, 480, false);
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

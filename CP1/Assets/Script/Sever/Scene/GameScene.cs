using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class GameScene : BaseScene
    {
        protected override void Init()
        {
            base.Init();
            SceneType = Define.Scene.Game;

            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.SetResolution(1280, 720, false);

            Application.runInBackground = true;
        }

        public override void Clear()
        {

        }
    }
}
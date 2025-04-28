using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class LobbyScene : BaseScene
    {
        UI_BackGround _backScene;

        protected override void Init()
        {
            base.Init();

            SceneType = Define.Scene.Lobby;

            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.SetResolution(1280, 720, false);

            Application.runInBackground = true;

            _backScene = Managers.UI.ShowSceneUI<UI_BackGround>("UI_BackGround");
        }

        public override void Clear()
        {

        }
    }
}
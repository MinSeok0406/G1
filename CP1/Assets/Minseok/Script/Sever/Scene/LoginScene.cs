using Minseok;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class LoginScene : BaseScene
    {
        UI_LoginScene _sceneUI;

        protected override void Init()
        {
            base.Init();

            SceneType = Define.Scene.Login;

            Managers.Web.BaseUrl = "https://localhost:5001/api";

            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.SetResolution(1280, 720, false);

            _sceneUI = Managers.UI.ShowSceneUI<UI_LoginScene>("UI_Login");
        }

        public override void Clear()
        {

        }
    }
}
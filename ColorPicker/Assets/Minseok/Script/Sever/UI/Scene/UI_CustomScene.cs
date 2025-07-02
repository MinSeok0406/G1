using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minseok
{
    public class UI_CustomScene : UI_Base
    {
        enum Images
        {
            Cancle,
        }

        public override void Init()
        {
            Bind<Image>(typeof(Images));
            GetImage((int)Images.Cancle).gameObject.BindEvent(Cancle);
        }

        public void Cancle(PointerEventData evt)
        {
            UI_BackGround backSceneUI = Managers.UI.SceneUI as UI_BackGround;
            UI_LobbyScene lobbyUI = backSceneUI.mLobbyUI;

            if (!lobbyUI.gameObject.activeSelf)
            {
                lobbyUI.gameObject.SetActive(true);
                Managers.UI.ShowPopupUI<UI_Joystick>("UI_Joystick");
                gameObject.SetActive(false);
            }
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minseok
{
    public class UI_InGameScene : UI_Base
    {
        enum Images
        {
            InLocal_Button,
            InOnline_Button,
            Cancle,
        }

        public override void Init()
        {
            Bind<Image>(typeof(Images));

            GetImage((int)Images.InLocal_Button).gameObject.BindEvent(InLocal);
            GetImage((int)Images.InOnline_Button).gameObject.BindEvent(InOnline);
            GetImage((int)Images.Cancle).gameObject.BindEvent(Cancle);
        }

        public void InLocal(PointerEventData evt)
        {
            // TODO Minseok
            Debug.Log("로컬로 고우~~");
        }

        public void InOnline(PointerEventData evt)
        {
            // TODO Minseok
            Debug.Log("온라인으로 고우~~");
        }

        public void Cancle(PointerEventData evt)
        {
            UI_BackGround backSceneUI = Managers.UI.SceneUI as UI_BackGround;
            UI_LobbyScene lobbyUI = backSceneUI.mLobbyUI;

            if (lobbyUI.gameObject.activeSelf)
            {
                lobbyUI.gameObject.SetActive(false);
                gameObject.SetActive(true);
            }
            else
            {
                lobbyUI?.gameObject.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}
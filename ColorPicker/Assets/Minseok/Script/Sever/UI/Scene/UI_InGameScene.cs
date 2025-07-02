using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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
            UI_BackGround backSceneUI = Managers.UI.SceneUI as UI_BackGround;
            UI_LobbyScene lobbyUI = backSceneUI.mLobbyUI;
            lobbyUI.gameObject.SetActive(false);
            Managers.UI.ClosePopupUI();

            Debug.Log("로컬로 고우~~");
            //PhotonNetwork.LoadLevel("InLocal");
            Managers.Object.RemoveMyPlayer();
            Managers.Scene.LoadScene(Define.Scene.InLocal);
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

            if (!lobbyUI.gameObject.activeSelf)
            {
                lobbyUI.gameObject.SetActive(true);
                Managers.UI.ShowPopupUI<UI_Joystick>("UI_Joystick");
                gameObject.SetActive(false);
            }
        }
    }
}
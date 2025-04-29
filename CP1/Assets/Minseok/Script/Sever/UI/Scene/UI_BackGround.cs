using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class UI_BackGround : UI_Scene
    {
        public UI_LobbyScene mLobbyUI { get; private set; }
        public UI_ChatScene ChatUI { get; private set; }
        public UI_CustomScene CustomUI { get; private set; }
        public UI_InGameScene InGameUI { get; private set; }

        public override void Init()
        {
            base.Init();

            mLobbyUI = GetComponentInChildren<UI_LobbyScene>();
            ChatUI = GetComponentInChildren<UI_ChatScene>();
            CustomUI = GetComponentInChildren<UI_CustomScene>();
            InGameUI = GetComponentInChildren<UI_InGameScene>();

            mLobbyUI.gameObject.SetActive(true);
            ChatUI.gameObject.SetActive(false);
            CustomUI.gameObject.SetActive(false);
            InGameUI.gameObject.SetActive(false);
        }
    }
}
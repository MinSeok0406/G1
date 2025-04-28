using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minseok
{
    public class UI_LoginScene : UI_Scene
    {
        public ServerInfo Info { get; set; }

        enum GameObjects
        {
            AccountName
        }

        enum Images
        {
            CreateBtn,
            LoginBtn
        }

        public override void Init()
        {
            base.Init();

            Bind<GameObject>(typeof(GameObjects));
            Bind<Image>(typeof(Images));

            GetImage((int)Images.CreateBtn).gameObject.BindEvent(OnClickCreateButton);
            GetImage((int)Images.LoginBtn).gameObject.BindEvent(OnClickLoginButton);
        }

        public void OnClickCreateButton(PointerEventData evt)
        {
            string account = Get<GameObject>((int)GameObjects.AccountName).GetComponent<TMP_InputField>().text;

            if (account == null)
            {
                Debug.Log("½ÇÆÐ");
                return;
            }

            CreateAccountPacketReq packet = new CreateAccountPacketReq()
            {
                AccountName = account,
            };

            Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, (res) =>
            {
                Debug.Log(res.CreateOk);

                Get<GameObject>((int)GameObjects.AccountName).GetComponent<TMP_InputField>().text = "";
            });
        }

        public void OnClickLoginButton(PointerEventData evt)
        {
            string account = Get<GameObject>((int)GameObjects.AccountName).GetComponent<TMP_InputField>().text;

            LoginAccountPacketReq packet = new LoginAccountPacketReq()
            {
                AccountName = account,
            };

            Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, (res) =>
            {
                Debug.Log(res.LoginOk);

                Get<GameObject>((int)GameObjects.AccountName).GetComponent<TMP_InputField>().text = "";

                if (res.LoginOk)
                {
                    Managers.Network.AccountId = res.AccountId;
                    Managers.Network.Token = res.Token;


                    for (int i = 0; i < res.ServerList.Count; i++)
                    {
                        Info = res.ServerList[i];
                    }

                    Managers.Network.ConnectToGame(Info);
                    Managers.Scene.LoadScene(Define.Scene.Lobby);
                }

                /*if (res.LoginOk)
                {
                    Managers.Network.AccountId = res.AccountId;
                    Managers.Network.Token = res.Token;

                    UI_SelectServerPopup popup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
                    popup.SetServers(res.ServerList);
                }*/

            });
        }
    }
}
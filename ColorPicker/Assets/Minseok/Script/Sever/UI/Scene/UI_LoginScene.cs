using GooglePlayGames;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using GooglePlayGames.BasicApi;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minseok
{
    public class UI_LoginScene : UI_Scene
    {
        public ServerInfo Info { get; set; }

        public override void Init()
        {
            base.Init();

            PlayGamesPlatform.Instance.Authenticate(OnClickCreateButton);
        }

        internal void OnClickCreateButton(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
                string account = PlayGamesPlatform.Instance.GetUserId();

                if (account == null)
                {
                    Debug.Log("½ÇÆÐ");
                    return;
                }

                CreateAccountPacketReq packet = new CreateAccountPacketReq()
                {
                    AccountName = displayName,
                    GoogleID = account,
                };

                Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, (res) =>
                {
                    Debug.Log(res.CreateOk);
                });
            }
        }

        public void OnClickLoginButton()
        {
            string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            string account = PlayGamesPlatform.Instance.GetUserId();

            LoginAccountPacketReq packet = new LoginAccountPacketReq()
            {
                AccountName = displayName,
                GoogleID = account,
            };

            Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, (res) =>
            {
                Debug.Log(res.LoginOk);

                if (res.LoginOk)
                {
                    Managers.Network.AccountId = res.AccountId;
                    Managers.Network.Token = res.Token;
                    Managers.Network.GoogleID = res.GoogleID;
                    Managers.Network.UserName = res.Name;

                    for (int i = 0; i < res.ServerList.Count; i++)
                    {
                        Info = res.ServerList[i];
                    }

                    Managers.Network.ConnectToGame(Info);
                    Managers.Scene.LoadScene(Define.Scene.Lobby);
                }

            });
        }
    }
}
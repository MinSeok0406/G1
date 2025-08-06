using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Principal;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minseok
{
    public class UI_LoginScene : UI_Scene
    {
        [SerializeField] 
        private TMP_Text googleID;

        [SerializeField]
        private TMP_Text Name;

        [SerializeField] 
        private GameObject Button;

        [SerializeField]
        private TMP_Text test;

        public ServerInfo Info { get; set; }

        public override void Init()
        {
            base.Init();

            Button.SetActive(false);
            PlayGamesPlatform.Instance.Authenticate(OnClickCreateButton);
        }

        internal void OnClickCreateButton(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
                string account = PlayGamesPlatform.Instance.GetUserId();

                googleID.text = account;
                Name.text = displayName;

                if (account == null)
                {
                    Debug.Log("실패");
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
                    test.text = res.CreateOk.ToString();
                });

                Button.SetActive(true);
            }
            else
            {
                Debug.Log("로그인 실패");
                googleID.text = "가져오기 실패";
                Name.text = "가져오기 실패";
                test.text = "가져오기 실패";
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
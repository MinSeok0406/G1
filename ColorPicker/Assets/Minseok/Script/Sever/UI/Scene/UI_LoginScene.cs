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

        [SerializeField]
        private TMP_Text test2;

        public ServerInfo Info { get; set; }

        public override void Init()
        {
            base.Init();

            Managers.Web.ConfigureBase("https", ServerConfig.Host, ServerConfig.Port, "/api/");

            Button.SetActive(false);
            PlayGamesPlatform.Instance.Authenticate(OnClickCreateButton);
        }

        internal void OnClickCreateButton(SignInStatus status)
        {
            if (status == SignInStatus.Success)
            {
                test2.text = Managers.Web.BuildUrl("account/create");

                string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
                string account = PlayGamesPlatform.Instance.GetUserId();

                googleID.text = account ?? "(null)";
                Name.text = displayName ?? "(null)";

                if (string.IsNullOrEmpty(account))
                {
                    Debug.Log("구글 로그인은 성공했지만 account(UserId)가 없음");
                    test.text = "account가 없음";
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
                    Button.SetActive(true);
                });

                test.text = ServerConfig.BaseUri.ToString();
            }
            else
            {
                Debug.Log("로그인 실패");
                googleID.text = "가져오기 실패";
                Name.text = "가져오기 실패";
                test.text = "가져오기 실패";
            }
        }

        // 버튼 클릭 시 작동
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
                    test.text = "LoginOk 여기까지 옴";
                    Managers.Network.AccountId = res.AccountId;
                    Managers.Network.Token = res.Token;
                    Managers.Network.GoogleID = res.GoogleID;
                    Managers.Network.UserName = res.Name;

                    ServerInfo chosen = null;
                    if (res.ServerList != null && res.ServerList.Count > 0)
                    {
                        res.ServerList.Sort((a, b) => a.BusyScore.CompareTo(b.BusyScore));
                        chosen = res.ServerList[0];
                        test.text = "chosen 없어서 여기 옴";
                    }
                    else
                    {
                        chosen = new ServerInfo
                        {
                            Name = "Fallback",
                            IpAddress = ServerConfig.GameHostFallback,
                            Port = ServerConfig.GamePortFallback,
                            BusyScore = 0
                        };

                        test.text = "chosen 있어서 여기 옴";
                    }

                    Info = chosen;

                    Managers.Network.ConnectToGame(Info);
                    Managers.Scene.LoadScene(Define.Scene.Lobby);
                }

            });
        }
    }
}
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
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
                string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
                string account = PlayGamesPlatform.Instance.GetUserId();

                googleID.text = account ?? "(null)";
                Name.text = displayName ?? "(null)";

                if (string.IsNullOrEmpty(account))
                {
                    Debug.Log("구글 로그인은 성공했지만 account(UserId)가 없음");
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
                    Button.SetActive(true);
                });
            }
            else
            {
                Debug.Log("로그인 실패");
                googleID.text = "가져오기 실패";
                Name.text = "가져오기 실패";
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
                try
                {
                    Debug.Log($"[LOGIN] ok={res?.LoginOk}, id={res?.AccountId}, name={res?.Name}");

                    if (res == null)
                    {
                        Debug.LogError("[LOGIN] Response object is null");
                        return;
                    }

                    if (!res.LoginOk)
                    {
                        return;
                    }

                    // 1) Managers.Network 존재 확인
                    if (Managers.Network == null)
                    {
                        Debug.LogError("[LOGIN] Managers.Network is NULL. 네트워크 매니저 프리팹/오브젝트가 씬에 있는지, 초기화됐는지 확인하세요.");
                        return;
                    }

                    // 2) 필드 설정
                    Managers.Network.AccountId = res.AccountId;
                    Managers.Network.Token = res.Token;
                    Managers.Network.GoogleID = res.GoogleID;
                    Managers.Network.UserName = res.Name;

                    // 3) 서버 선택
                    ServerInfo chosen = null;
                    var list = res.ServerList;
                    if (list != null)
                    {
                        // 잘못된 엔트리 제거(포트 0, 주소 빈 값 등)
                        list.RemoveAll(s => string.IsNullOrWhiteSpace(s.IpAddress) || s.Port <= 0);
                    }

                    if (list != null && list.Count > 0)
                    {
                        list.Sort((a, b) => a.BusyScore.CompareTo(b.BusyScore));
                        chosen = list[0];
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
                    }

                    if (chosen == null)
                    {
                        Debug.LogError("[LOGIN] No server available (chosen == null)");
                        return;
                    }

                    Debug.Log($"[LOGIN] chosen server = {chosen.Name} {chosen.IpAddress}:{chosen.Port}");

                    Info = chosen;

                    Managers.Network.ConnectToGame(Info);
                    Managers.Scene.LoadScene(Define.Scene.Lobby);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LOGIN][HANDLER] {ex}");
                }

            });
        }
    }
}
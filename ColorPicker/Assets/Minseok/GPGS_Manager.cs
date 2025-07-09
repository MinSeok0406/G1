using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using TMPro;
using GooglePlayGames.BasicApi;

public class GPGS_Manager : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;

    void Start()
    {
        
    }

    public void GPGS_LogIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            string userID = PlayGamesPlatform.Instance.GetUserId();

            logText.text = "로그인 성공 : " + displayName + " / " + userID;
        }
        else
        {
            logText.text = "로그인 실패";
        }
    }

    public void ShowAchievementUI()
    {
        // 전체 업적 표시
        PlayGamesPlatform.Instance.ShowAchievementsUI();
    }

    public void UnlockingGPGSAchievement()
    {
        // 업적 잠금 해제 및 공개
        PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement, (bool success) => { });
    }
}

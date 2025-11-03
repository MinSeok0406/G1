using System.Collections.Generic;
using FunkyCode.LightingSettings;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class MeetingUI : MonoBehaviour
    {
        [SerializeField] private Transform playerProfileContainer; // Transform로 받는 게 안전
        [SerializeField] private GameObject playerProfilePrefab;

        private Dictionary<int, MeetingProfileUI> meetingProfiles =  new Dictionary<int, MeetingProfileUI>();
        
        
        public void ClearAllProfilesSafe()
        {
            if (!playerProfileContainer) return;
            for (int i = playerProfileContainer.childCount - 1; i >= 0; i--)
            {
                var child = playerProfileContainer.GetChild(i);
                if (child) Destroy(child.gameObject);
            }
            meetingProfiles.Clear();
        }

        public void CreatePlayerCard(int actorNum, string nickname, bool isAlive)
        {
            if (!playerProfileContainer || !playerProfilePrefab)
            {
                Debug.LogError("[MeetingUI] Container or Prefab is not assigned.");
                return;
            }

            var go = Instantiate(playerProfilePrefab, playerProfileContainer);
            var profileUI = go.GetComponent<MeetingProfileUI>();
            if (!profileUI)
            {
                Debug.LogError("[MeetingUI] MeetingProfileUI component missing on prefab.");
                Destroy(go);
                return;
            }

            profileUI.Initialize(actorNum, nickname, isAlive);
            meetingProfiles[actorNum] = profileUI;
        }

        /// <summary>
        /// 특정 플레이어의 득표수 업데이트
        /// </summary>
        public void UpdatePlayerVoteCount(int actorNum, int voteCount)
        {
            if (meetingProfiles.TryGetValue(actorNum, out var profile))
            {
                profile.UpdateVoteCount(voteCount);
            }
            else
            {
                Debug.LogWarning($"[MeetingUI] Cannot update vote count for Actor {actorNum} - profile not found.");
            }
        }

        /// <summary>
        /// 모든 플레이어의 투표 표시 초기화
        /// </summary>
        public void ClearAllVotes()
        {
            foreach (var profile in meetingProfiles.Values)
            {
                if (profile != null)
                {
                    profile.ClearVotes();
                }
            }
        }

        public void ClearPlayerCard()
        {
            foreach (var actorNum in meetingProfiles.Keys)
            {
                var profile = meetingProfiles[actorNum];
                meetingProfiles.Remove(actorNum);
                Destroy(profile);
            }
        }
    }
}

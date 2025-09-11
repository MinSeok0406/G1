using System.Collections.Generic;
using FunkyCode.LightingSettings;
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.inGame
{
    public class MeetingUI : MonoBehaviourPun
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

            profileUI.Initialize(actorNum,nickname, isAlive);
            meetingProfiles[actorNum] = profileUI;
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

using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    public class DeductionUI : MonoBehaviour
    {
        [SerializeField] private Transform playerProfileContainer;
        [SerializeField] private GameObject playerProfilePrefab;
        
        private Dictionary<int, PlayerCardUI> playerCardUIMap = new Dictionary<int, PlayerCardUI>();

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
            var playerCardUI = go.GetComponent<PlayerCardUI>();
            if (!playerCardUI)
            {
                Debug.LogError("[MeetingUI] MeetingProfileUI component missing on prefab.");
                Destroy(go);
                return;
            }

            playerCardUI.Initialize(actorNum, nickname, isAlive);
            playerCardUIMap[actorNum] = playerCardUI;
        }

        public void ClearPlayerCard()
        {
            foreach (var actorNum in playerCardUIMap.Keys)
            {
                var playerCard = playerCardUIMap[actorNum];
                playerCardUIMap.Remove(actorNum);
                Destroy(playerCard);
            }
        }
    }
}
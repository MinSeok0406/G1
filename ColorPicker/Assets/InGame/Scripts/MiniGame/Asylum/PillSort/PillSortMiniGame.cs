using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 약물 분류 미니게임 (정신병원 테마)
    /// 흩어진 약을 색상/타입별로 분류하여 정리
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class PillSortMiniGame : MiniGameBase
    {
        [SerializeField] private Transform pillsParent;
        [SerializeField] private List<PillContainer> containers = new(); // 약 보관함들

        private List<PillObject> pills = new();
        private int sortedCount = 0;
        private int playerId;
        private MiniGameTag miniGameTag;

        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

            sortedCount = 0;
            pills.Clear();

            foreach (Transform child in pillsParent)
            {
                if (child.TryGetComponent(out PillObject pill))
                {
                    pill.OnPillSorted += OnPillSorted;
                    pills.Add(pill);
                }
            }
        }

        private void OnPillSorted()
        {
            sortedCount++;

            if (sortedCount >= pills.Count)
            {
                MiniGameReport repo = new MiniGameReport()
                {
                    playerId = playerId,
                    miniGameType = miniGameTag.miniGameType,
                    success = true
                };

                onComplete?.Invoke(repo);
                Debug.Log("[PillSort] All pills sorted!");
            }
        }

        public override void StartGame()
        {
            foreach (PillObject pill in pills)
            {
                pill.gameObject.SetActive(true);
                pill.ResetToRandomPosition();
            }
        }

        public void ResetMission()
        {
            sortedCount = 0;

            foreach (PillObject pill in pills)
            {
                pill.gameObject.SetActive(true);
                pill.ResetToRandomPosition();
            }

            Debug.Log("[PillSort] Mission reset");
        }

        private void OnDestroy()
        {
            foreach (var pill in pills)
            {
                pill.OnPillSorted -= OnPillSorted;
            }
        }
    }
}

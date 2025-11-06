using System;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 카드 긁기 미니게임
    /// 카드를 오른쪽으로 드래그하여 인식시키는 게임
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class CardSwipeMiniGame : MiniGameBase
    {
        [SerializeField] private CardObject card;
        [SerializeField] private float swipeDistance = 200f; // 성공 거리

        private int playerId;
        private MiniGameTag miniGameTag;
        private bool isCompleted = false;

        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
            isCompleted = false;

            if (card != null)
            {
                card.OnSwipeSuccess += OnCardSwipeSuccess;
                card.SetSwipeDistance(swipeDistance);
            }
        }

        private void OnCardSwipeSuccess()
        {
            if (isCompleted) return;

            isCompleted = true;

            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = true
            };

            onComplete?.Invoke(repo);
        }

        public override void StartGame()
        {
            if (card != null)
            {
                card.gameObject.SetActive(true);
                card.ResetCard();
            }
        }

        /// <summary>
        /// 미션 초기화
        /// </summary>
        public void ResetMission()
        {
            isCompleted = false;
            if (card != null)
            {
                card.ResetCard();
            }
        }

        private void OnDestroy()
        {
            if (card != null)
            {
                card.OnSwipeSuccess -= OnCardSwipeSuccess;
            }
        }
    }
}

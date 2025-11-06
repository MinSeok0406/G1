using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 클릭해서 깨는 미니게임
    /// 모든 타겟을 클릭해서 파괴하면 성공
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class ClickBreakMiniGame : MiniGameBase
    {
        [Header("UI Components")]
        [SerializeField] private List<ClickBreakTarget> targets = new(); // 타겟 리스트

        [Header("Game Settings")]
        [SerializeField] private int targetsToBreak = 5; // 깨야 할 타겟 수

        private int brokenCount = 0;
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

            // 타겟 이벤트 연결
            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.OnTargetBroken += OnTargetBroken;
                }
                else
                {
                    Debug.LogWarning("[ClickBreak] Target is null!", this);
                }
            }
        }

        public override void StartGame()
        {
            brokenCount = 0;

            // 모든 타겟 리셋
            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.ResetTarget();
                }
            }

            Debug.Log($"[ClickBreak] Game started! Break {targetsToBreak} targets.");
        }

        private void OnTargetBroken()
        {
            brokenCount++;
            Debug.Log($"[ClickBreak] Target broken! ({brokenCount}/{targetsToBreak})");

            if (brokenCount >= targetsToBreak)
            {
                CompleteGame();
            }
        }

        private void CompleteGame()
        {
            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = true
            };

            onComplete?.Invoke(repo);
            Debug.Log("[ClickBreak] All targets broken! Game complete!");
        }

        public void ResetMission()
        {
            brokenCount = 0;

            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.ResetTarget();
                }
            }

            Debug.Log("[ClickBreak] Mission reset");
        }

        private void OnDestroy()
        {
            // 이벤트 정리
            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.OnTargetBroken -= OnTargetBroken;
                }
            }
        }
    }
}

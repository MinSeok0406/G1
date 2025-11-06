using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 순서대로 등장하는 클릭 미니게임
    /// 타겟이 순서대로 나타나고 모두 클릭하면 성공
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class SequenceClickMiniGame : MiniGameBase
    {
        [Header("UI Components")]
        [SerializeField] private List<SequenceClickTarget> targets = new(); // 타겟 리스트

        [Header("Game Settings")]
        [SerializeField] private int targetsToClick = 5; // 클릭해야 할 타겟 수
        [SerializeField] private float spawnDelay = 0.5f; // 타겟 등장 간격

        private int currentSequence = 0;
        private int clickedCount = 0;
        private int playerId;
        private MiniGameTag miniGameTag;
        private bool isGameActive = false;
        private float spawnTimer = 0f;

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
                    target.OnTargetClicked += OnTargetClicked;
                }
                else
                {
                    Debug.LogWarning("[SequenceClick] Target is null!", this);
                }
            }
        }

        public override void StartGame()
        {
            currentSequence = 0;
            clickedCount = 0;
            isGameActive = true;
            spawnTimer = 0f;

            // 모든 타겟 비활성화
            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.ResetTarget();
                }
            }

            Debug.Log($"[SequenceClick] Game started! Click {targetsToClick} targets in sequence.");
        }

        private void Update()
        {
            if (!isGameActive) return;

            spawnTimer += Time.deltaTime;

            // 일정 간격으로 타겟 활성화
            if (currentSequence < targetsToClick && spawnTimer >= spawnDelay)
            {
                spawnTimer = 0f;
                ActivateNextTarget();
            }
        }

        private void ActivateNextTarget()
        {
            if (currentSequence >= targets.Count)
            {
                Debug.LogWarning($"[SequenceClick] Not enough targets! Need {targetsToClick}, have {targets.Count}");
                return;
            }

            var target = targets[currentSequence];
            if (target != null)
            {
                target.Activate(currentSequence + 1); // 1부터 시작하는 번호
                currentSequence++;
            }
        }

        private void OnTargetClicked(int sequenceNumber)
        {
            clickedCount++;
            Debug.Log($"[SequenceClick] Target clicked! ({clickedCount}/{targetsToClick})");

            if (clickedCount >= targetsToClick)
            {
                CompleteGame();
            }
        }

        private void CompleteGame()
        {
            isGameActive = false;

            MiniGameReport repo = new MiniGameReport()
            {
                playerId = playerId,
                miniGameType = miniGameTag.miniGameType,
                success = true
            };

            onComplete?.Invoke(repo);
            Debug.Log("[SequenceClick] All targets clicked! Game complete!");
        }

        public void ResetMission()
        {
            currentSequence = 0;
            clickedCount = 0;
            isGameActive = false;
            spawnTimer = 0f;

            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.ResetTarget();
                }
            }

            Debug.Log("[SequenceClick] Mission reset");
        }

        private void OnDestroy()
        {
            // 이벤트 정리
            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.OnTargetClicked -= OnTargetClicked;
                }
            }
        }
    }
}

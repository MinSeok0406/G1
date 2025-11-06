using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 심박수 모니터링 미니게임 (정신병원 테마)
    /// 심박수를 모니터링하고 비정상 심박을 클릭하여 처리
    /// 5번 안에 무조건 비정상 심박이 나타나며, 한 번 찾으면 성공
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class HeartRateMonitorMiniGame : MiniGameBase
    {
        [SerializeField] private HeartRateDisplay display;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private float abnormalProbability = 0.3f; // 30% 확률로 비정상 발생
        [SerializeField] private int maxAttemptsBeforeForce = 5; // 5번 안에 무조건 비정상 발생

        private int playerId;
        private MiniGameTag miniGameTag;
        private float spawnTimer = 0f;
        private bool isGameActive = false;
        private int attemptCount = 0; // 시도 횟수 카운트

        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

            if (display != null)
            {
                display.OnAbnormalFixed += OnAbnormalFixed;
            }
        }

        private void OnAbnormalFixed()
        {
            // 한 번 찾으면 성공
            CompleteGame();
        }

        public override void StartGame()
        {
            isGameActive = true;
            spawnTimer = 0f;
            attemptCount = 0;

            if (display != null)
            {
                display.ResetDisplay();
            }
        }

        private void Update()
        {
            if (!isGameActive) return;

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnAbnormalHeartbeat();
            }
        }

        private void SpawnAbnormalHeartbeat()
        {
            if (display == null) return;

            attemptCount++;

            // 5번째 시도거나 랜덤 확률에 걸리면 비정상 심박 표시
            bool shouldShowAbnormal = attemptCount >= maxAttemptsBeforeForce ||
                                     Random.value < abnormalProbability;

            if (shouldShowAbnormal)
            {
                display.ShowAbnormal();
                Debug.Log($"[HeartRateMonitor] Abnormal spawned on attempt {attemptCount}");
            }
            else
            {
                display.ShowNormal();
                Debug.Log($"[HeartRateMonitor] Normal heartbeat shown (attempt {attemptCount})");
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
            Debug.Log("[HeartRateMonitor] All abnormal heartbeats fixed!");
        }

        public void ResetMission()
        {
            isGameActive = false;
            spawnTimer = 0f;
            attemptCount = 0;

            if (display != null)
            {
                display.ResetDisplay();
            }

            Debug.Log("[HeartRateMonitor] Mission reset");
        }

        private void OnDestroy()
        {
            if (display != null)
            {
                display.OnAbnormalFixed -= OnAbnormalFixed;
            }
        }
    }
}

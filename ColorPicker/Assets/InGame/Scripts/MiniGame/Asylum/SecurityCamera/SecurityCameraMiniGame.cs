using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 감시 카메라 미니게임
    /// 여러 카메라 화면에서 이상 행동을 발견하여 클릭
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class SecurityCameraMiniGame : MiniGameBase
    {
        [SerializeField] private List<CameraFeed> cameraFeeds = new();
        [SerializeField] private int anomaliesToFind = 5;
        [SerializeField] private float spawnInterval = 3f;

        private int foundCount = 0;
        private float spawnTimer = 0f;
        private bool isGameActive = false;
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

            foreach (var feed in cameraFeeds)
            {
                feed.OnAnomalyClicked += OnAnomalyFound;
            }
        }

        public override void StartGame()
        {
            foundCount = 0;
            isGameActive = true;
            spawnTimer = 0f;

            foreach (var feed in cameraFeeds)
            {
                feed.HideAnomaly();
            }
        }

        private void Update()
        {
            if (!isGameActive) return;

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnAnomaly();
            }
        }

        private void SpawnAnomaly()
        {
            var normalFeeds = cameraFeeds.FindAll(f => !f.HasAnomaly());
            if (normalFeeds.Count > 0)
            {
                int randomIndex = Random.Range(0, normalFeeds.Count);
                normalFeeds[randomIndex].ShowAnomaly();
            }
        }

        private void OnAnomalyFound()
        {
            foundCount++;

            if (foundCount >= anomaliesToFind)
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
        }

        public void ResetMission()
        {
            foundCount = 0;
            isGameActive = false;
            spawnTimer = 0f;

            foreach (var feed in cameraFeeds)
            {
                feed.HideAnomaly();
            }
        }
    }

    /// <summary>
    /// 카메라 피드 (간단 버전)
    /// </summary>
    public class CameraFeed : MonoBehaviour
    {
        [SerializeField] private GameObject anomalyIndicator;
        private ClickableObject clickable;
        private bool hasAnomaly = false;
        public System.Action OnAnomalyClicked;

        private void Awake()
        {
            clickable = GetComponent<ClickableObject>();
            if (clickable != null)
            {
                clickable.OnClickEvent += OnClicked;
            }
        }

        private void OnClicked(Vector2 localPosition)
        {
            if (hasAnomaly)
            {
                HideAnomaly();
                OnAnomalyClicked?.Invoke();
            }
        }

        public void ShowAnomaly()
        {
            hasAnomaly = true;
            if (anomalyIndicator != null)
                anomalyIndicator.SetActive(true);
        }

        public void HideAnomaly()
        {
            hasAnomaly = false;
            if (anomalyIndicator != null)
                anomalyIndicator.SetActive(false);
        }

        public bool HasAnomaly() => hasAnomaly;
    }
}

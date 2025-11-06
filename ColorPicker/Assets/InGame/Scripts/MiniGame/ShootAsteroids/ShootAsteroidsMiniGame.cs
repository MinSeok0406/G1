using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 운석 슈팅 미니게임 (레트로 스타일)
    /// 화면에 나타나는 운석을 클릭하여 파괴
    /// </summary>
    [RequireComponent(typeof(MiniGameTag))]
    public class ShootAsteroidsMiniGame : MiniGameBase
    {
        [SerializeField] private Transform asteroidsParent;
        [SerializeField] private int asteroidsToDestroy = 20;
        [SerializeField] private float spawnInterval = 0.5f;

        private List<AsteroidObject> asteroids = new();
        private int destroyedCount = 0;
        private int playerId;
        private MiniGameTag miniGameTag;

        private float spawnTimer = 0f;
        private bool isGameActive = false;

        private void Awake()
        {
            Initialize();
        }

        public override void Initialize()
        {
            miniGameTag = GetComponent<MiniGameTag>();
            playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

            destroyedCount = 0;

            // 운석 수집
            asteroids.Clear();
            foreach (Transform child in asteroidsParent)
            {
                if (child.TryGetComponent(out AsteroidObject asteroid))
                {
                    asteroid.OnAsteroidDestroyed += OnAsteroidDestroyed;
                    asteroids.Add(asteroid);
                }
            }
        }

        public override void StartGame()
        {
            destroyedCount = 0;
            isGameActive = true;
            spawnTimer = 0f;

            // 모든 운석 비활성화
            foreach (var asteroid in asteroids)
            {
                asteroid.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isGameActive) return;

            // 운석 스폰
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnAsteroid();
            }
        }

        private void SpawnAsteroid()
        {
            // 비활성화된 운석 찾기
            var inactiveAsteroid = asteroids.Find(a => !a.gameObject.activeSelf);
            if (inactiveAsteroid != null)
            {
                inactiveAsteroid.Spawn();
            }
        }

        private void OnAsteroidDestroyed()
        {
            destroyedCount++;

            if (destroyedCount >= asteroidsToDestroy)
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
            Debug.Log("[ShootAsteroids] All asteroids destroyed!");
        }

        /// <summary>
        /// 미션 초기화
        /// </summary>
        public void ResetMission()
        {
            destroyedCount = 0;
            isGameActive = false;
            spawnTimer = 0f;

            foreach (var asteroid in asteroids)
            {
                asteroid.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            foreach (var asteroid in asteroids)
            {
                asteroid.OnAsteroidDestroyed -= OnAsteroidDestroyed;
            }
        }
    }
}

using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 라운드 시작 및 초기화 관리
    /// - 플레이어 스폰
    /// - 미션 할당 (생존자만)
    /// - 능력 쿨타임 초기화
    /// - 페인트 초기화
    /// </summary>
    public sealed class RoundManager : SingletonNetworkBehaviour<RoundManager>
    {
        #region Events
        public event Action OnRoundStarted;
        public event Action OnRoundEnded;
        #endregion

        #region Serialized Fields
        [Header("Spawn Settings")]
        [SerializeField] private Transform[] spawnPoints; // 스폰 위치 배열
        [SerializeField] private bool useRandomSpawn = true; // 랜덤 스폰 사용 여부

        [Header("Round Settings")]
        [SerializeField] private float roundStartDelay = 2f; // 라운드 시작 전 딜레이
        #endregion

        #region Private Fields
        private int _currentRound = 0;
        private bool _isRoundActive = false;
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
        }
        #endregion

        #region Public API
        /// <summary>
        /// [Host Only] 새 라운드 시작
        /// </summary>
        public void StartNewRound()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[RoundManager] StartNewRound is host-only.");
                return;
            }

            if (_isRoundActive)
            {
                Debug.LogWarning("[RoundManager] Round is already active.");
                return;
            }

            _currentRound++;
            _isRoundActive = true;

            Debug.Log($"[RoundManager] Starting Round {_currentRound}");

            StartCoroutine(Co_InitializeRound());
        }

        /// <summary>
        /// [Host Only] 라운드 종료
        /// </summary>
        public void EndRound()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[RoundManager] EndRound is host-only.");
                return;
            }

            if (!_isRoundActive)
            {
                Debug.LogWarning("[RoundManager] No active round to end.");
                return;
            }

            _isRoundActive = false;

            Debug.Log($"[RoundManager] Ending Round {_currentRound}");

            OnRoundEnded?.Invoke();

            // 브로드캐스트
            photonView.RPC(nameof(RPC_OnRoundEnded), RpcTarget.All);
        }

        /// <summary>
        /// 현재 라운드 번호 반환
        /// </summary>
        public int GetCurrentRound() => _currentRound;

        /// <summary>
        /// 라운드 활성 상태 반환
        /// </summary>
        public bool IsRoundActive() => _isRoundActive;
        #endregion

        #region Round Initialization
        /// <summary>
        /// [Host Only] 라운드 초기화 코루틴
        /// </summary>
        private IEnumerator Co_InitializeRound()
        {
            // 1. 라운드 시작 알림
            photonView.RPC(nameof(RPC_OnRoundStarting), RpcTarget.All, _currentRound);

            yield return new WaitForSeconds(roundStartDelay);

            // 2. 플레이어 스폰 위치 초기화
            InitializePlayerSpawns();

            // 3. 미션 초기화 (생존자만)
            InitializeMissions();

            // 4. 능력 쿨타임 초기화
            InitializeAbilityCooldowns();

            // 5. 페인트 초기화 (죽은 플레이어 색상 리셋)
            ResetDeadPlayerPaintColors();

            // 6. 시체 정리
            ClearDeathBodies();

            // 7. 승리 조건 상태 리셋
            ResetVictoryConditions();

            // 8. 라운드 시작 완료 알림
            OnRoundStarted?.Invoke();
            photonView.RPC(nameof(RPC_OnRoundStarted), RpcTarget.All, _currentRound);

            Debug.Log($"[RoundManager] Round {_currentRound} initialized successfully.");
        }

        /// <summary>
        /// [Host Only] 플레이어 스폰 위치 초기화
        /// </summary>
        private void InitializePlayerSpawns()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[RoundManager] No spawn points configured. Using default spawn.");
                return;
            }

            var allPlayers = GameDataManager.Instance?.GetAllPublicPlayerData();
            if (allPlayers == null || allPlayers.Count == 0)
                return;

            List<Transform> availableSpawns = new List<Transform>(spawnPoints);

            foreach (var playerData in allPlayers)
            {
                if (playerData == null) continue;

                // PhotonView 찾기
                if (!GameDataManager.Instance.TryGetViewIDByUID(playerData.googleUID, out int viewID))
                    continue;

                var photonView = PhotonView.Find(viewID);
                if (photonView == null)
                    continue;

                // Owner 확인
                if (photonView.Owner == null)
                {
                    Debug.LogWarning($"[RoundManager] PhotonView.Owner is null for player {playerData.googleUID}");
                    continue;
                }

                // 스폰 위치 선택
                Transform spawnTransform = SelectSpawnPoint(availableSpawns);
                if (spawnTransform == null)
                {
                    Debug.LogWarning($"[RoundManager] No available spawn point for player {playerData.googleUID}");
                    continue;
                }

                // 플레이어 이동 RPC - 각 플레이어의 Owner에게 전송
                Vector3 spawnPosition = spawnTransform.position;

                var datas = GameDataManager.Instance.GetAllPublicPlayerData();

                foreach(var data in datas)
                {
                    PhotonView view = GameDataManager.Instance.GetPhotonViewByActorId(data.currentActorId);
                    view.gameObject.transform.position = spawnPosition;
                }

                Debug.Log($"[RoundManager] Spawning player {playerData.googleUID} (Actor {photonView.Owner.ActorNumber}) at {spawnPosition}");
            }
        }

        /// <summary>
        /// 스폰 포인트 선택
        /// </summary>
        private Transform SelectSpawnPoint(List<Transform> availableSpawns)
        {
            if (availableSpawns.Count == 0)
                return null;

            int index = useRandomSpawn
                ? UnityEngine.Random.Range(0, availableSpawns.Count)
                : 0;

            Transform selected = availableSpawns[index];
            availableSpawns.RemoveAt(index);

            return selected;
        }

        /// <summary>
        /// [Host Only] 미션 초기화 (생존자만)
        /// </summary>
        private void InitializeMissions()
        {
            if (MissionManager.Instance == null)
            {
                Debug.LogWarning("[RoundManager] MissionManager not found.");
                return;
            }

            // 생존자 목록 가져오기
            var alivePlayers = GetAlivePlayers();
            if (alivePlayers.Count == 0)
            {
                Debug.LogWarning("[RoundManager] No alive players for mission assignment.");
                return;
            }

            // 미션 할당
            MissionManager.Instance.InitializeRoundMissions(alivePlayers);

            Debug.Log($"[RoundManager] Missions initialized for {alivePlayers.Count} alive players.");
        }

        /// <summary>
        /// 생존 플레이어 목록 반환
        /// </summary>
        private List<PublicPlayerData> GetAlivePlayers()
        {
            var result = new List<PublicPlayerData>();
            var allPlayers = GameDataManager.Instance?.GetAllPublicPlayerData();

            if (allPlayers == null) return result;

            foreach (var player in allPlayers)
            {
                if (player == null) continue;

                var inGameData = GameDataManager.Instance.GetInGameData(player.googleUID);
                if (inGameData != null && inGameData.isAlive)
                {
                    result.Add(player);
                }
            }

            return result;
        }

        /// <summary>
        /// [Host Only] 능력 쿨타임 초기화
        /// </summary>
        private void InitializeAbilityCooldowns()
        {
            if (AbilityManager.Instance == null)
            {
                Debug.LogWarning("[RoundManager] AbilityManager not found.");
                return;
            }

            var rules = GameDataManager.Instance?.GetGameRules();
            if (rules == null)
            {
                Debug.LogWarning("[RoundManager] GameRules not found.");
                return;
            }

            var allPlayers = GameDataManager.Instance?.GetAllPublicPlayerData();
            if (allPlayers == null) return;

            foreach (var player in allPlayers)
            {
                if (player == null) continue;

                // 죽은 플레이어는 스킵
                var inGameData = GameDataManager.Instance.GetInGameData(player.googleUID);
                if (inGameData == null || !inGameData.isAlive)
                    continue;

                if (!GameDataManager.Instance.TryGetViewIDByUID(player.googleUID, out int viewID))
                    continue;

                if (!GameDataManager.Instance.TryGetPrivatePlayerData(player.googleUID, out var privateData))
                    continue;

                var classType = (PlayerClassType)privateData.classType;

                // 마피아: 50% 쿨타임으로 시작
                if (classType == PlayerClassType.mafia)
                {
                    float initialCooldown = rules.mafiaKillCooldown * rules.mafiaInitialCooldownPercent;
                    AbilityManager.Instance.StartCooldown(viewID, initialCooldown);
                    Debug.Log($"[RoundManager] Mafia {player.googleUID} started with {initialCooldown}s cooldown.");
                }
                // 탐정: 능력 리셋 (즉시 사용 가능)
                else if (classType == PlayerClassType.detective)
                {
                    AbilityManager.Instance.ResetDetectiveAbility(viewID);
                    Debug.Log($"[RoundManager] Detective {player.googleUID} ability reset.");
                }
            }
        }

        /// <summary>
        /// [Host Only] 죽은 플레이어의 페인트 색상 리셋
        /// </summary>
        private void ResetDeadPlayerPaintColors()
        {
            if (ColorObjectManager.Instance == null)
            {
                Debug.LogWarning("[RoundManager] ColorObjectManager not found.");
                return;
            }

            ColorObjectManager.Instance.ResetDeadPlayersColors();
        }

        /// <summary>
        /// [Host Only] 시체 정리
        /// </summary>
        private void ClearDeathBodies()
        {
            if (DeathBodyManager.Instance == null)
            {
                Debug.LogWarning("[RoundManager] DeathBodyManager not found.");
                return;
            }

            DeathBodyManager.Instance.ClearAllDeathBodies();
        }

        /// <summary>
        /// [Host Only] 승리 조건 상태 리셋
        /// </summary>
        private void ResetVictoryConditions()
        {
            if (VictoryConditionManager.Instance == null)
            {
                Debug.LogWarning("[RoundManager] VictoryConditionManager not found.");
                return;
            }

            VictoryConditionManager.Instance.ResetGameEndState();
        }
        #endregion

        #region RPC
        [PunRPC]
        private void RPC_OnRoundStarting(int roundNumber)
        {
            Debug.Log($"[RoundManager] Round {roundNumber} is starting...");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowToastToScreen($"라운드 {roundNumber} 시작!");
            }
        }

        [PunRPC]
        private void RPC_OnRoundStarted(int roundNumber)
        {
            _currentRound = roundNumber;
            _isRoundActive = true;

            Debug.Log($"[RoundManager] Round {roundNumber} started!");

            OnRoundStarted?.Invoke();
        }

        [PunRPC]
        private void RPC_OnRoundEnded()
        {
            _isRoundActive = false;

            Debug.Log($"[RoundManager] Round {_currentRound} ended.");

            OnRoundEnded?.Invoke();
        }

        [PunRPC]
        private void RPC_TeleportPlayer(Vector3 position)
        {
            var player = PlayerManager.Instance?.GetMyPlayer();
            if (player == null)
            {
                Debug.LogWarning("[RoundManager] Cannot find local player for teleport.");
                return;
            }

            player.transform.position = position;
            Debug.Log($"[RoundManager] Teleported to {position}");
        }
        #endregion

        #region Debug
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugStartRound()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[RoundManager] Debug command is host-only.");
                return;
            }

            StartNewRound();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugEndRound()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[RoundManager] Debug command is host-only.");
                return;
            }

            EndRound();
        }
        #endregion
    }
}

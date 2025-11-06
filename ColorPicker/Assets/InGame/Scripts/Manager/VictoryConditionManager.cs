using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 승리 조건 관리 및 체크
    /// - 시민승: 모든 마피아 죽음 OR 모든 페인트 칠해짐
    /// - 마피아승: 페인트 칠해지기 전에 모든 시민 죽음
    /// </summary>
    public sealed class VictoryConditionManager : SingletonNetworkBehaviour<VictoryConditionManager>
    {
        #region Events
        /// <summary>
        /// 게임 종료 이벤트 (winningTeam: 0=시민, 1=마피아)
        /// </summary>
        public event Action<int> OnGameEnd;
        #endregion

        #region Constants
        private const int TEAM_CITIZEN = 0;
        private const int TEAM_MAFIA = 1;
        #endregion

        #region Private Fields
        private bool _isGameEnded = false;
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
        }
        #endregion

        #region Public API
        /// <summary>
        /// [Host Only] 승리 조건 체크 - 플레이어 사망 시 호출
        /// </summary>
        public void CheckVictoryConditionOnDeath()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[VictoryConditionManager] CheckVictoryCondition is host-only.");
                return;
            }

            if (_isGameEnded)
            {
                Debug.Log("[VictoryConditionManager] Game already ended.");
                return;
            }

            CheckVictoryCondition();
        }

        /// <summary>
        /// [Host Only] 승리 조건 체크 - 페인트 칠해질 때 호출
        /// </summary>
        public void CheckVictoryConditionOnPaint()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[VictoryConditionManager] CheckVictoryCondition is host-only.");
                return;
            }

            if (_isGameEnded)
            {
                Debug.Log("[VictoryConditionManager] Game already ended.");
                return;
            }

            CheckVictoryCondition();
        }

        /// <summary>
        /// 게임 종료 상태 초기화 (새 라운드 시작 시)
        /// </summary>
        public void ResetGameEndState()
        {
            _isGameEnded = false;
        }
        #endregion

        #region Victory Check Logic
        /// <summary>
        /// [Host Only] 승리 조건 종합 체크
        /// </summary>
        private void CheckVictoryCondition()
        {
            // 1. 모든 플레이어 데이터 가져오기
            var allPlayers = GameDataManager.Instance?.GetAllPublicPlayerData();
            if (allPlayers == null || allPlayers.Count == 0)
            {
                Debug.LogWarning("[VictoryConditionManager] No player data available.");
                return;
            }

            // 2. 생존자와 직업별 분류
            var alivePlayersByClass = ClassifyAlivePlayers(allPlayers);
            if (alivePlayersByClass == null)
            {
                Debug.LogWarning("[VictoryConditionManager] Failed to classify players.");
                return;
            }

            int aliveMafiaCount = alivePlayersByClass.ContainsKey(PlayerClassType.mafia)
                ? alivePlayersByClass[PlayerClassType.mafia].Count : 0;
            int aliveCitizenCount = GetAliveCitizenCount(alivePlayersByClass);

            Debug.Log($"[VictoryConditionManager] Alive - Mafia: {aliveMafiaCount}, Citizens: {aliveCitizenCount}");

            // 3. 승리 조건 체크
            // 조건 1: 모든 마피아 사망 → 시민 승
            if (aliveMafiaCount == 0)
            {
                TriggerGameEnd(TEAM_CITIZEN, "모든 마피아가 처치되었습니다!");
                return;
            }

            // 조건 2: 모든 시민 사망 → 마피아 승
            if (aliveCitizenCount == 0)
            {
                TriggerGameEnd(TEAM_MAFIA, "모든 시민이 사망했습니다!");
                return;
            }

            // 조건 3: 모든 페인트 칠해짐 → 시민 승
            if (CheckAllPaintCompleted())
            {
                TriggerGameEnd(TEAM_CITIZEN, "모든 페인트가 칠해졌습니다!");
                return;
            }
        }

        /// <summary>
        /// 생존 플레이어를 직업별로 분류
        /// </summary>
        private Dictionary<PlayerClassType, List<PublicPlayerData>> ClassifyAlivePlayers(List<PublicPlayerData> allPlayers)
        {
            var result = new Dictionary<PlayerClassType, List<PublicPlayerData>>();

            foreach (var player in allPlayers)
            {
                if (player == null) continue;

                // InGame 데이터 확인 (생존 여부)
                var inGameData = GameDataManager.Instance.GetInGameData(player.googleUID);
                if (inGameData == null || !inGameData.isAlive)
                    continue;

                // Private 데이터 확인 (직업)
                if (!GameDataManager.Instance.TryGetPrivatePlayerData(player.googleUID, out var privateData))
                    continue;

                var classType = (PlayerClassType)privateData.classType;

                if (!result.ContainsKey(classType))
                    result[classType] = new List<PublicPlayerData>();

                result[classType].Add(player);
            }

            return result;
        }

        /// <summary>
        /// 생존 시민 수 계산 (마피아가 아닌 모든 직업)
        /// </summary>
        private int GetAliveCitizenCount(Dictionary<PlayerClassType, List<PublicPlayerData>> classified)
        {
            int count = 0;
            foreach (var kvp in classified)
            {
                if (kvp.Key != PlayerClassType.mafia)
                    count += kvp.Value.Count;
            }
            return count;
        }

        /// <summary>
        /// 모든 페인트가 칠해졌는지 체크
        /// </summary>
        private bool CheckAllPaintCompleted()
        {
            var paintDict = GameDataManager.Instance?.GetAllPaintObjectDictionary();
            if (paintDict == null || paintDict.Count == 0)
            {
                // 페인트 오브젝트가 없거나 등록되지 않음 → false
                return false;
            }

            foreach (var kvp in paintDict)
            {
                int colorType = kvp.Value;
                // -1은 칠해지지 않은 상태
                if (colorType == -1)
                    return false;
            }

            // 모든 오브젝트가 칠해짐
            return true;
        }
        #endregion

        #region Game End
        /// <summary>
        /// [Host Only] 게임 종료 트리거
        /// </summary>
        private void TriggerGameEnd(int winningTeam, string message)
        {
            if (_isGameEnded)
                return;

            _isGameEnded = true;

            Debug.Log($"[VictoryConditionManager] Game End - Winner: {(winningTeam == TEAM_CITIZEN ? "시민" : "마피아")} | {message}");

            // 이벤트 발생
            OnGameEnd?.Invoke(winningTeam);

            // 모든 클라이언트에 결과 브로드캐스트
            photonView.RPC(nameof(RPC_GameEnd), RpcTarget.All, winningTeam, message);
        }

        [PunRPC]
        private void RPC_GameEnd(int winningTeam, string message)
        {
            _isGameEnded = true;

            Debug.Log($"[VictoryConditionManager] Received Game End - Winner: {(winningTeam == TEAM_CITIZEN ? "시민" : "마피아")} | {message}");

            // UI 표시
            ShowGameResultUI(winningTeam, message);

            // 게임 스테이트 변경 (Result 스테이트로 전환)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RequestPhaseChange(GameStateType.Result);
            }
        }

        /// <summary>
        /// 게임 결과 UI 표시
        /// </summary>
        private void ShowGameResultUI(int winningTeam, string message)
        {
            if (UIManager.Instance == null)
            {
                Debug.LogWarning("[VictoryConditionManager] UIManager not found.");
                return;
            }

            // TODO: UIManager에 게임 결과 UI 표시 메서드 추가 필요
            string winnerText = winningTeam == TEAM_CITIZEN ? "시민 승리!" : "마피아 승리!";
            string fullMessage = $"{winnerText}\n{message}";

            UIManager.Instance.ShowToastToScreen(fullMessage);

            Debug.Log($"[VictoryConditionManager] Displaying result UI: {fullMessage}");
        }
        #endregion

        #region Debug
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugForceWin(int team)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[VictoryConditionManager] Debug command is host-only.");
                return;
            }

            string message = team == TEAM_CITIZEN ? "Debug: Forced Citizen Win" : "Debug: Forced Mafia Win";
            TriggerGameEnd(team, message);
        }
        #endregion
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 투표 시스템 관리 클래스 (투표 로직 캡슐화)
    /// </summary>
    public sealed class VotingSystemManager
    {
        #region Private Fields
        private readonly HashSet<int> _voters = new();
        private readonly Dictionary<int, int> _voteCounts = new();
        #endregion

        #region Public API
        /// <summary>
        /// [Host Only] 투표 처리 (중복 투표 방지)
        /// </summary>
        /// <param name="voterActorNumber">투표자 ActorNumber</param>
        /// <param name="targetActorNumber">투표 대상 ActorNumber</param>
        /// <param name="totalPlayers">전체 플레이어 수</param>
        /// <returns>전원 투표 완료 여부</returns>
        public bool ProcessVote(int voterActorNumber, int targetActorNumber, int totalPlayers)
        {
            // 죽은 플레이어의 투표 차단
            if (GameDataManager.Instance.TryGetInGameDataByActorId(voterActorNumber, out var voterData))
            {
                if (!voterData.isAlive)
                {
                    Debug.LogWarning($"[Vote] Actor {voterActorNumber} is dead and cannot vote.");
                    return false;
                }
            }

            // 중복 투표 방지
            if (!_voters.Add(voterActorNumber))
            {
                Debug.LogWarning($"[Vote] Actor {voterActorNumber} already voted.");
                return false;
            }

            // 투표 집계
            if (_voteCounts.ContainsKey(targetActorNumber))
            {
                _voteCounts[targetActorNumber]++;
            }
            else
            {
                _voteCounts[targetActorNumber] = 1;
            }

            Debug.Log($"[Vote] Actor {voterActorNumber} voted for Actor {targetActorNumber}. Current votes: {_voteCounts[targetActorNumber]}");

            // 전원 투표 완료 확인 (생존한 플레이어 수 기준)
            bool allVoted = CheckAllVoted(totalPlayers);

            if (allVoted)
            {
                LogVoteResults();
            }

            return allVoted;
        }

        /// <summary>
        /// 최다 득표자 반환 (동표 시 -1)
        /// </summary>
        public int GetMostVotedPlayer()
        {
            if (_voteCounts.Count == 0)
            {
                Debug.Log("[Vote] No votes recorded.");
                return -1;
            }

            int maxVotes = -1;
            int mostVotedActor = -1;
            int tieCount = 0;

            // 최대 득표수 찾기
            foreach (var kvp in _voteCounts)
            {
                if (kvp.Value > maxVotes)
                {
                    maxVotes = kvp.Value;
                }
            }

            // 최다 득표자 확인 및 동점 체크
            foreach (var kvp in _voteCounts)
            {
                if (kvp.Value == maxVotes)
                {
                    mostVotedActor = kvp.Key;
                    tieCount++;
                }
            }

            // 동점자 2명 이상 시 -1 반환
            if (tieCount > 1)
            {
                Debug.Log($"[Vote] Tie detected: {tieCount} players with {maxVotes} votes each.");
                return -1;
            }

            return mostVotedActor;
        }

        /// <summary>
        /// 투표 결과 딕셔너리 반환 (읽기 전용 복사본)
        /// </summary>
        public Dictionary<int, int> GetVoteDictionary()
        {
            return new Dictionary<int, int>(_voteCounts);
        }

        /// <summary>
        /// 투표한 플레이어 수
        /// </summary>
        public int GetVotedPlayerCount()
        {
            return _voters.Count;
        }

        /// <summary>
        /// 투표 데이터 초기화
        /// </summary>
        public void ResetVotes()
        {
            _voters.Clear();
            _voteCounts.Clear();

            UIManager.Instance.BroadCastResetVoteState();
        }
        #endregion

        #region Private Helpers
        /// <summary>
        /// 전원 투표 완료 확인 (생존한 플레이어만 카운트)
        /// </summary>
        private bool CheckAllVoted(int totalPlayers)
        {
            if (totalPlayers <= 0) return false;

            // 생존한 플레이어 수 계산
            int alivePlayerCount = 0;
            var allPlayers = GameDataManager.Instance?.GetAllPublicPlayerData();
            if (allPlayers != null)
            {
                foreach (var player in allPlayers)
                {
                    if (player == null) continue;
                    var inGameData = GameDataManager.Instance.GetInGameData(player.googleUID);
                    if (inGameData != null && inGameData.isAlive)
                    {
                        alivePlayerCount++;
                    }
                }
            }

            // 생존한 플레이어 수가 0이면 전체 플레이어 수 사용 (초기화 전)
            int requiredVotes = alivePlayerCount > 0 ? alivePlayerCount : totalPlayers;
            return _voters.Count >= requiredVotes;
        }

        /// <summary>
        /// 투표 결과 로그 출력
        /// </summary>
        private void LogVoteResults()
        {
            if (_voteCounts.Count == 0)
            {
                Debug.Log("[Vote Result] No votes recorded.");
                return;
            }

            Debug.Log("===== Vote Results =====");

            foreach (var kvp in _voteCounts)
            {
                Debug.Log($"Actor {kvp.Key}: {kvp.Value} vote(s)");
            }

            int mostVoted = GetMostVotedPlayer();

            if (mostVoted == -1)
            {
                Debug.Log("[Vote Result] No elimination due to tie.");
            }
            else
            {
                Debug.Log($"[Vote Result] Most voted player: Actor {mostVoted}");
            }
        }
        
        #endregion
    }
}
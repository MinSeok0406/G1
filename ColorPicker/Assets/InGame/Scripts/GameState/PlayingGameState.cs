using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// PlayingGameState에 미션 재배분 로직 추가
    /// </summary>
    public partial class PlayingGameState : GameState
    {
        public PlayingGameState(GameStateMachine stateMachine) : base(stateMachine)
        {
        }

        /// <summary>
        /// 라운드 시작 시 호출 - 미션 재배분
        /// </summary>
        public override void Enter()
        {
            base.Enter();
            InitializeRoundMissions();

            GameManager.Instance._voting.ResetVotes();
            GameManager.Instance._meetingTimer.ResetRequests();

            // PlayerCardUI와 DeductionUI 최신화 (죽은 플레이어 상태 반영)
            if (Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                UIManager.Instance?.InitializePlayerProfile();

                // 죽은 플레이어의 색을 모두 색칠하지 않은 상태로 리셋
                ColorObjectManager.Instance?.ResetDeadPlayersColors();

                // 모든 시체 제거 (투표 처형 후 생성된 시체 포함, 딜레이 적용)
                GameManager.Instance?.ClearDeathBodiesDelayed(0.5f);
            }
        }

        /// <summary>
        /// 라운드 시작 시 생존 플레이어에게 미션 재배분
        /// </summary>
        private void InitializeRoundMissions()
        {
            if (!Photon.Pun.PhotonNetwork.IsMasterClient)
            {
                Debug.Log("[PlayingGameState] Mission initialization skipped (not master client).");
                return;
            }

            var alivePlayers = GetAlivePlayers();
            
            if (alivePlayers == null || alivePlayers.Count == 0)
            {
                Debug.LogWarning("[PlayingGameState] No alive players to assign missions.");
                return;
            }

            MissionManager.Instance?.InitializeRoundMissions(alivePlayers);
            
            
            Debug.Log($"[PlayingGameState] Missions redistributed to {alivePlayers.Count} alive players.");
        }

        /// <summary>
        /// 생존한 플레이어 목록 조회
        /// </summary>
        private List<PublicPlayerData> GetAlivePlayers()
        {
            var allPlayers = GameDataManager.Instance?.GetAllPublicPlayerData();
            if (allPlayers == null) return new List<PublicPlayerData>();

            var alivePlayers = new List<PublicPlayerData>();

            foreach (var playerData in allPlayers)
            {
                if (playerData == null) continue;

                var inGameData = GameDataManager.Instance.GetInGameData(playerData.googleUID);
                
                if (inGameData != null && inGameData.isAlive)
                {
                    alivePlayers.Add(playerData);
                }
            }

            return alivePlayers;
        }
    }
}
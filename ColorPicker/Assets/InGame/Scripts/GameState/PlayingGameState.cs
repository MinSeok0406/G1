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
using Photon.Pun;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 승리 조건 체크 클래스
    /// </summary>
    public sealed class WinConditionChecker
    {
        /// <summary>
        /// 승리 조건 확인 (호스트만)
        /// </summary>
        public bool CheckWinCondition(out GameResult result)
        {
            result = GameResult.None;

            if (!PhotonNetwork.IsMasterClient) return false;
            if (GameDataManager.Instance == null) return false;

            // 1. 시민팀 승리 조건: 모든 마피아 사망 또는 모든 페인트 오브젝트 칠해짐
            if (AreAllMafiaDead())
            {
                result = GameResult.CitizenWin;
                return true;
            }

            if (AreAllPaintObjectsPainted())
            {
                result = GameResult.CitizenWin;
                return true;
            }

            // 2. 마피아팀 승리 조건: 모든 시민 사망
            if (AreAllCitizensDead())
            {
                result = GameResult.MafiaWin;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 모든 마피아가 사망했는지 확인
        /// </summary>
        private bool AreAllMafiaDead()
        {
            var allPlayers = GameDataManager.Instance.GetAllPublicPlayerData();
            if (allPlayers == null || allPlayers.Count == 0) return false;

            int mafiaCount = 0;
            int deadMafiaCount = 0;

            foreach (var playerData in allPlayers)
            {
                if (playerData == null) continue;

                if (!GameDataManager.Instance.TryGetPrivatePlayerData(playerData.googleUID, out var privateData))
                    continue;

                if (privateData.classType == (int)PlayerClassType.mafia)
                {
                    mafiaCount++;

                    var inGameData = GameDataManager.Instance.GetInGameData(playerData.googleUID);
                    if (inGameData != null && !inGameData.isAlive)
                    {
                        deadMafiaCount++;
                    }
                }
            }

            // 마피아가 1명 이상 있고, 모두 사망했으면 true
            return mafiaCount > 0 && mafiaCount == deadMafiaCount;
        }

        /// <summary>
        /// 모든 페인트 오브젝트가 칠해졌는지 확인
        /// </summary>
        private bool AreAllPaintObjectsPainted()
        {
            var paintDict = GameDataManager.Instance.GetAllPaintObjectDictionary();
            if (paintDict == null || paintDict.Count == 0) return false;

            int totalPaintObjects = paintDict.Count;
            int paintedObjectsCount = 0;

            foreach (var kvp in paintDict)
            {
                // colorId가 -1이 아니면 칠해진 것
                if (kvp.Value >= 0)
                {
                    paintedObjectsCount++;
                }
            }

            // 모든 페인트 오브젝트가 칠해졌으면 true
            return totalPaintObjects > 0 && paintedObjectsCount == totalPaintObjects;
        }

        /// <summary>
        /// 모든 시민이 사망했는지 확인
        /// </summary>
        private bool AreAllCitizensDead()
        {
            var allPlayers = GameDataManager.Instance.GetAllPublicPlayerData();
            if (allPlayers == null || allPlayers.Count == 0) return false;

            int citizenCount = 0;
            int deadCitizenCount = 0;

            foreach (var playerData in allPlayers)
            {
                if (playerData == null) continue;

                if (!GameDataManager.Instance.TryGetPrivatePlayerData(playerData.googleUID, out var privateData))
                    continue;

                // 마피아가 아닌 플레이어 (시민, 탐정 등)
                if (privateData.classType != (int)PlayerClassType.mafia &&
                    privateData.classType != (int)PlayerClassType.ghost)
                {
                    citizenCount++;

                    var inGameData = GameDataManager.Instance.GetInGameData(playerData.googleUID);
                    if (inGameData != null && !inGameData.isAlive)
                    {
                        deadCitizenCount++;
                    }
                }
            }

            // 시민이 1명 이상 있고, 모두 사망했으면 true
            return citizenCount > 0 && citizenCount == deadCitizenCount;
        }
    }

    /// <summary>
    /// 게임 결과
    /// </summary>
    public enum GameResult
    {
        None,
        CitizenWin,
        MafiaWin
    }
}

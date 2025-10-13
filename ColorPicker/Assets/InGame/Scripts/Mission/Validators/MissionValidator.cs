using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 미션 완료 요청 검증 클래스
    /// </summary>
    public sealed class MissionValidator : IMissionValidator
    {
        /// <summary>
        /// 미션 완료 시도 및 유효성 검증
        /// </summary>
        /// <param name="missions">플레이어의 미션 목록</param>
        /// <param name="type">완료하려는 미션 타입</param>
        /// <param name="index">완료된 미션의 인덱스 (out)</param>
        /// <returns>성공 여부</returns>
        public bool TryCompleteMission(List<MissionInstance> missions, MiniGameType type, out int index)
        {
            index = -1;

            if (missions == null || missions.Count == 0)
            {
                Debug.LogWarning("[MissionValidator] Mission list is null or empty.");
                return false;
            }

            // 해당 타입의 미완료 미션 찾기
            for (int i = 0; i < missions.Count; i++)
            {
                var mission = missions[i];
                
                if (mission.missionType == type && !mission.isCompleted)
                {
                    index = i;
                    return true;
                }
            }

            Debug.LogWarning($"[MissionValidator] No pending mission of type {type} found.");
            return false;
        }

        /// <summary>
        /// 개별 미션의 유효성 검증
        /// </summary>
        public bool IsMissionValid(MissionInstance mission)
        {
            if (string.IsNullOrEmpty(mission.missionId))
            {
                Debug.LogWarning("[MissionValidator] Mission ID is invalid.");
                return false;
            }

            return true;
        }
    }
}
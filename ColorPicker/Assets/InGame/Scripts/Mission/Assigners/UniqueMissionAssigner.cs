using System.Collections.Generic;
using UnityEngine;

namespace ColorPicker.InGame
{
    /// <summary>
    /// Fisher-Yates 셔플을 사용한 유니크 미션 할당자
    /// </summary>
    public sealed class UniqueMissionAssigner : IMissionAssigner
    {
        private readonly List<MiniGameTemplate> _availableMissions;

        public UniqueMissionAssigner(List<MiniGameTemplate> availableMissions)
        {
            _availableMissions = availableMissions ?? new List<MiniGameTemplate>();
        }

        /// <summary>
        /// 플레이어에게 유니크한 미션 k개 할당 (Fisher-Yates 셔플 사용)
        /// </summary>
        public List<MissionInstance> AssignMissions(string playerUID, int count)
        {
            if (_availableMissions == null || _availableMissions.Count == 0)
            {
                Debug.LogWarning($"[MissionAssigner] No available missions to assign.");
                return new List<MissionInstance>();
            }

            int totalMissions = _availableMissions.Count;
            int missionsToAssign = Mathf.Clamp(count, 1, totalMissions);

            var indices = ArrayPoolHelper.Rent(totalMissions);
            var missions = new List<MissionInstance>(missionsToAssign);

            try
            {
                // 인덱스 배열 초기화
                for (int i = 0; i < totalMissions; i++)
                {
                    indices[i] = i;
                }

                // Fisher-Yates 셔플 (부분 셔플)
                for (int i = 0; i < missionsToAssign; i++)
                {
                    int randomIndex = Random.Range(i, totalMissions);
                    
                    // Swap
                    (indices[i], indices[randomIndex]) = (indices[randomIndex], indices[i]);
                }

                // 할당된 미션 생성
                for (int i = 0; i < missionsToAssign; i++)
                {
                    var template = _availableMissions[indices[i]];
                    missions.Add(new MissionInstance
                    {
                        missionId = template.miniGameName,
                        missionType = template.miniGameType,
                        isCompleted = false
                    });
                }

                Debug.Log($"[MissionAssigner] Assigned {missions.Count} missions to player {playerUID}");
                return missions;
            }
            finally
            {
                ArrayPoolHelper.Return(indices);
            }
        }
    }

    /// <summary>
    /// Thread-local 배열 풀 (GC 최소화)
    /// </summary>
    internal static class ArrayPoolHelper
    {
        [System.ThreadStatic]
        private static int[] _buffer;

        public static int[] Rent(int size)
        {
            if (_buffer == null || _buffer.Length < size)
            {
                _buffer = new int[size];
            }
            return _buffer;
        }

        public static void Return(int[] array)
        {
            // Thread-local storage이므로 별도 반환 작업 불필요
        }
    }
}
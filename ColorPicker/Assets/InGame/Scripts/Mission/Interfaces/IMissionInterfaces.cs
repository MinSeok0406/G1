using System.Collections.Generic;

namespace ColorPicker.InGame
{
    /// <summary>
    /// 미션 할당 전략 인터페이스
    /// </summary>
    public interface IMissionAssigner
    {
        List<MissionInstance> AssignMissions(string playerUID, int count);
    }

    /// <summary>
    /// 미션 검증 인터페이스
    /// </summary>
    public interface IMissionValidator
    {
        bool TryCompleteMission(List<MissionInstance> missions, MiniGameType type, out int index);
        bool IsMissionValid(MissionInstance mission);
    }

    /// <summary>
    /// 미션 보상 제공 인터페이스
    /// </summary>
    public interface IMissionRewardProvider
    {
        MissionReward GetMissionReward();
    }

    /// <summary>
    /// 미션 보상 데이터
    /// </summary>
    public struct MissionReward
    {
        public int coinAmount;
        public int experienceAmount;
    }
}
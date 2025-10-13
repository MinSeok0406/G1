namespace ColorPicker.InGame
{
    /// <summary>
    /// 미션 보상 제공 클래스
    /// </summary>
    public sealed class MissionRewardProvider : IMissionRewardProvider
    {
        private readonly int _defaultCoinReward;
        private readonly int _defaultExperienceReward;

        public MissionRewardProvider(int coinReward = 10, int experienceReward = 5)
        {
            _defaultCoinReward = coinReward;
            _defaultExperienceReward = experienceReward;
        }

        /// <summary>
        /// 기본 미션 보상 반환
        /// </summary>
        public MissionReward GetMissionReward()
        {
            return new MissionReward
            {
                coinAmount = _defaultCoinReward,
                experienceAmount = _defaultExperienceReward
            };
        }
    }
}
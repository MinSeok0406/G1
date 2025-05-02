using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Achievement
    {
        public AchievementInfo Info { get; } = new AchievementInfo();

        public int AchievementDbId
        {
            get { return Info.AchievementDbId; }
            set { Info.AchievementDbId = value; }
        }

        public int TemplateId
        {
            get { return Info.TemplateId; }
            set { Info.TemplateId = value; }
        }

        public string IsActive
        {
            get { return Info.Locked; }
            set { Info.Locked = value; }
        }

        public Achievement(int achievementDbId)
        {
            Init(achievementDbId);
        }

        void Init(int achievementDbId)
        {
            AchievementData achievementData = null;
            DataManager.AchievementDict.TryGetValue(achievementDbId, out achievementData);

            AchievementData data = (AchievementData)achievementData;
            {
                TemplateId = data.id;
                IsActive = data.isActive;
            }
        }

        public static Achievement GetAchievement(AchievementDb achievementDb)
        {
            Achievement achievement = null;

            AchievementData achievementData = null;
            DataManager.AchievementDict.TryGetValue(achievementDb.AchievementDbId, out achievementData);

            if (achievementData == null)
                return null;

            achievement = new Achievement(achievementDb.TemplateId);

            if (achievement == null)
                return null;

            achievement.AchievementDbId = achievementDb.AchievementDbId;
            achievement.IsActive = achievementDb.IsActive;

            return achievement;
        }
    }
}

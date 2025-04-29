using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Slot
    {
        Dictionary<int, Achievement> _achievements = new Dictionary<int, Achievement>();

        public void Add(Achievement achievement)
        {
            _achievements.Add(achievement.AchievementDbId, achievement);
        }

        public Achievement Get(int achievementDbId)
        {
            Achievement achievement = null;
            _achievements.TryGetValue(achievementDbId, out achievement);
            return achievement;
        }

        public Achievement Find(Func<Achievement, bool> condition)
        {
            foreach (Achievement achievement in _achievements.Values)
            {
                if (condition.Invoke(achievement))
                    return achievement;
            }

            return null;
        }
    }
}

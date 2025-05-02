using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    #region Stat
    public class Stat
    {
        public static float _speed = 7.0f;
    }

    /*[Serializable]
    public class StatData : ILoader<int, Stat>
    {
        public List<Stat> stats = new List<Stat>();

        public Dictionary<int, Stat> MakeDict()
        {
            Dictionary<int, Stat> dict = new Dictionary<int, Stat>();
            foreach (Stat stat in stats)
                dict.Add(stat.level, stat);
            return dict;
        }
    }*/
    #endregion

    #region Skill
    /*[Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public float cooldown;
        public int damage;
        public SkillType skillType;
        public ProjectileInfo projectile;
    }

    public class ProjectileInfo
    {
        public string name;
        public float speed;
        public int range;
        public string prefab;
    }

    [Serializable]
    public class SkillData : ILoader<int, Skill>
    {
        public List<Skill> skills = new List<Skill>();

        public Dictionary<int, Skill> MakeDict()
        {
            Dictionary<int, Skill> dict = new Dictionary<int, Skill>();
            foreach (Skill skill in skills)
                dict.Add(skill.id, skill);
            return dict;
        }
    }*/
    #endregion

    #region Achievement
    [Serializable]
    public class AchievementData
    {
        public int id;
        public string name;
        public string isActive;
    }

    [Serializable]
    public class ItemLoader : ILoader<int, AchievementData>
    {
        public List<AchievementData> achievements = new List<AchievementData>();

        public Dictionary<int, AchievementData> MakeDict()
        {
            Dictionary<int, AchievementData> dict = new Dictionary<int, AchievementData>();
            foreach (AchievementData achievement in achievements)
            {
                dict.Add(achievement.id, achievement);
            }

            return dict;
        }
    }
    #endregion
}

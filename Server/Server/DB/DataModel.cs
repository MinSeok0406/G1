using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server.DB
{
    [Table("Account")]
    public class AccountDb
    {
        public int AccountDbId { get; set; }
        public string AccountDbLogin { get; set; }
        public string AccountDbPassword { get; set; }
        public string AccountName { get; set; }
        public ICollection<PlayerDb> Players { get; set; }
    }

    [Table("Player")]
    public class PlayerDb
    {
        public int PlayerDbId { get; set; }
        public string PlayerName { get; set; }

        [ForeignKey("Account")]
        public int AccountDbId { get; set; }
        public AccountDb Account { get; set; }

        public ICollection<AchievementDb> Achievements { get; set; }

        public float Speed { get; set; }

        
    }

    [Table("Achievement")]
    public class AchievementDb
    {
        public int AchievementDbId { get; set; }
        public int TemplateId { get; set; }
        public int Slot { get; set; }
        public string IsActive { get; set; }

        [ForeignKey("Owner")]
        public int? OwnerDbId { get; set; }
        public PlayerDb Owner { get; set; }
    }
}

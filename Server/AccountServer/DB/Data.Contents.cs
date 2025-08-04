using System.ComponentModel.DataAnnotations.Schema;

namespace AccountServer.DB
{
    [Table("Account")]
    public class AccountDb
    {
        public int AccountDbId { get; set; }
        public string Name { get; set; }
        public string GoogleID { get; set; }
    }
}

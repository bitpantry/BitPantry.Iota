using System.Data.Common;

namespace BitPantry.Iota.Data.Entity
{
    public class User : EntityBase<long>
    {
        public string EmailAddress { get; set; }
        public DateTime LastLogin { get; set; }
        public List<Card> Cards { get; set; }
    }
}

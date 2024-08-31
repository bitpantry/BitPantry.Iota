
using BitPantry.Iota.Data.Entity;

namespace BitPantry.Iota.Web.Models
{
    public class CardModel
    {
        public long Id { get; internal set; }
        public DateTime AddedOn { get; internal set; }
        public DateTime LastMovedOn { get; internal set; }
        public Divider Divider { get; internal set; }
        public Dictionary<int, string> Verses { get; internal set; }
    }
}

using BitPantry.Iota.Common;
using Microsoft.Build.Framework.Profiler;

namespace BitPantry.Iota.Web.Models
{
    public class ReorderCardRequestModel
    {
        public long CardId { get; set; }
        public int NewOrder { get; set; }
        public string Divider { get; set; }

        public Divider GetDividerEnum()
            => Enum.Parse<Divider>(Divider);
    }
}

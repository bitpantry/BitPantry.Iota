using BitPantry.Iota.Common;

namespace BitPantry.Iota.Web.Models
{
    public class ReorderCardRequestModel
    {
        public long CardId { get; set; }
        public int NewOrder { get; set; }
        public string Tab { get; set; }

        public Tab GetTabEnum()
            => Enum.Parse<Tab>(Tab);
    }
}

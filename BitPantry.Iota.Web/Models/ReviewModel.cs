using BitPantry.Iota.Common;

namespace BitPantry.Iota.Web.Models
{
    public record ReviewModel(Dictionary<Tab, int> Path, Tab CurrentTab, int CurrentOrder, CardModel Card, string NextUrl) { }
}

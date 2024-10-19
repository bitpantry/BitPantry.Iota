
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Humanizer;

namespace BitPantry.Iota.Web.Models
{
    public record CardModel(
        long Id,
        DateTime AddedOn,
        DateTime LastMovedOn,
        DateTime LastReviewedOn,
        Divider Divider,
        int Order = 0,
        PassageModel Passage = null)
    {

        public string DividerDescription => Divider.Humanize();

    }
}

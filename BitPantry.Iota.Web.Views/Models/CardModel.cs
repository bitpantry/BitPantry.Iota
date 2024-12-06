using BitPantry.Iota.Common;
using Humanizer;

namespace BitPantry.Iota.Web.Models
{
    public record CardModel(
        long Id,
        string Address,
        DateTime AddedOn,
        DateTime LastMovedOn,
        DateTime? LastReviewedOn,
        Tab Tab,
        int Order = 0,
        PassageModel Passage = null)
    {

        public string TabDescription => Tab.Humanize();

    }
}

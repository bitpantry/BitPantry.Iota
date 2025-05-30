﻿using BitPantry.Tabs.Common;
using Humanizer;

namespace BitPantry.Tabs.Web.Models
{
    public record CardModel(
        long Id,
        string Address,
        DateTime AddedOn,
        DateTime LastMovedOn,
        DateTime? LastReviewedOn,
        Tab Tab,
        int ReviewCount,
        long RowNumber = 0,
        PassageModel Passage = null)
    {

        public string TabDescription => Tab.Humanize();

    }
}

using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public record GetCardResult(
        long Id,
        DateTime AddedOn,
        DateTime LastMovedOn,
        DateTime LastReviewedOn,
        Tab Tab,
        int Order,
        List<Verse> Verses)
    { }
}

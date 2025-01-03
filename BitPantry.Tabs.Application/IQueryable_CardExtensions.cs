using BitPantry.Tabs.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture;

namespace BitPantry.Tabs.Application
{
    public static class IQueryable_CardExtensions
    {
        //public static IQueryable<Card> IncludeRowNumber(this IQueryable<Card> dbSet)
        //    => dbSet
        //        .Select(c => new Card
        //        {
        //            Id = c.Id,
        //            UserId = c.UserId,
        //            Tab = c.Tab,
        //            FractionalOrder = c.FractionalOrder,
        //            AddedOn = c.AddedOn,
        //            LastMovedOn = c.LastMovedOn,
        //            LastReviewedOn = c.LastReviewedOn,
        //            Address = c.Address,
        //            BibleId = c.BibleId,
        //            StartVerseId = c.StartVerseId,
        //            EndVerseId = c.EndVerseId,
        //            ReviewCount = c.ReviewCount,
        //            RowNumber = EF.Functions.RowNumber(c.UserId, c.Tab, EF.Functions.OrderBy(c.FractionalOrder))
        //        });
    }
}

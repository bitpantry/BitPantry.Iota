using BitPantry.Iota.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public record CardHeader(long Id, DateTime AddedOn, DateTime LastMovedOn, DateTime LastReviewedOn, Divider Divider, int Order) { }
}
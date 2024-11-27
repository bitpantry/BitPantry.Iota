using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Microsoft.IdentityModel.Protocols.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Archive
{
    public class CardArchiveModel
    {
        public long BibleId { get; set; }
        public string Address { get; set; }
        public long StartVerseId { get; set; }
        public long EndVerseId { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime LastMovedOn { get; set; }
        public DateTime? LastReviewedOn { get; set; }
        public Tab Tab { get; set; }
        public int Order { get; set; }

        public Card ToEntity(long userId)
            => new()
            {
                UserId = userId,
                BibleId = BibleId,
                Address = Address,
                StartVerseId = StartVerseId,
                EndVerseId = EndVerseId,
                AddedOn = AddedOn,
                LastMovedOn = LastMovedOn,
                LastReviewedOn = LastReviewedOn,
                Tab = Tab,
                Order = Order
            };

    }
}

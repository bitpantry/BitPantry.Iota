using BitPantry.Tabs.Common;
using Microsoft.IdentityModel.Protocols.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Data.Entity
{

    public class Card : EntityBase<long>
    {
        public long UserId { get; set; }
        public User User { get; set; }
        public DateTime AddedOn { get; set; } = DateTime.UtcNow;
        public DateTime LastMovedOn { get; set; } = DateTime.UtcNow;
        public DateTime? LastReviewedOn { get; set; } = null;
        public string Address { get; set; }
        public long BibleId { get; set; }
        public long StartVerseId { get; set; }
        public long EndVerseId { get; set; }
        public Tab Tab { get; set; }
        public int ReviewCount { get; set; }
        public double FractionalOrder { get; set; }
        public NumberedCard NumberedCard { get; set; }
    }
}

using BitPantry.Iota.Common;
using Microsoft.IdentityModel.Protocols.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity
{

    public class Card : EntityBase<long>
    {
        public long UserId { get; set; }
        public User User { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime LastMovedOn { get; set; }
        public DateTime LastReviewedOn { get; set; }
        public List<Verse> Verses { get; set; } 
        public Tab Tab { get; set; }
        public int Order { get; set; }

        public string Thumbprint {
            get { return Verses == null ? null : ThumbprintUtil.Generate(Verses.Select(v => v.Id).ToList()); }
            set { _ = value; }
        }
    }
}

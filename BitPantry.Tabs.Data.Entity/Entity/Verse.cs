using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Data.Entity
{
    public class Verse : EntityBase<long>
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public long ChapterId { get; set; }
        public Chapter Chapter { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Data.Entity
{
    public class Book : EntityBase<long>
    {
        public int Number { get; set; }
        public long TestamentId { get; set; }
        public Testament Testament { get; set; }
        public List<Chapter> Chapters { get; set; }
    }
}

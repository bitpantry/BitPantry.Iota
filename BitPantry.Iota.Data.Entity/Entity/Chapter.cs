using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity
{
    public class Chapter : EntityBase<long>
    {
        public int Number { get; set; }
        public long BookId { get; set; }
        public Book Book { get; set; }
        public List<Verse> Verses { get; set; }
    }
}

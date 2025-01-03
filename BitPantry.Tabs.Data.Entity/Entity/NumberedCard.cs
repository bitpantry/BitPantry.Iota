using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Data.Entity
{
    public class NumberedCard 
    {
        public long CardId { get; set; }
        public long RowNumber { get; set; }
        public Card Card { get; set; }
    }
}

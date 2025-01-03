using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Data.Entity
{
    public enum TestamentName : int
    {
        Old = 0,
        New = 1
    }

    public class Testament : EntityBase<long>
    {
        public TestamentName Name { get; set; }
        public long BibleId { get; set; }
        public Bible Bible { get; set; }
        public List<Book> Books { get; set; }
    }
}

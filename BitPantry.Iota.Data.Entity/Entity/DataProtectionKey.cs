using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity
{
    public class DataProtectionKey : EntityBase<long>
    {
        public string Xml { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

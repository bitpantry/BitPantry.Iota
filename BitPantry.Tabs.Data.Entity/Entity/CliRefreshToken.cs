using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Data.Entity
{
    public class CliRefreshToken : EntityBase<long>
    {
        public string ClientId { get; set; }
        public string RefreshToken { get; set; }
    }
}

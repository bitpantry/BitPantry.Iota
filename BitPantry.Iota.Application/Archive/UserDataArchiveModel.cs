using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Archive
{
    public class UserDataArchiveModel
    {
        public UserArchiveModel User { get; set; }
        public List<CardArchiveModel> Cards { get; set; }
    }
}

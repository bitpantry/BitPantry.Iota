using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Archive
{
    public class UserArchiveModel
    {
        public long Id { get; set; }
        public string EmailAddress { get; set; }
        public DateTime LastLogin { get; set; }

        public User ToEntity()
            => new()
            {
                EmailAddress = EmailAddress,
                LastLogin = LastLogin
            };
    }
}

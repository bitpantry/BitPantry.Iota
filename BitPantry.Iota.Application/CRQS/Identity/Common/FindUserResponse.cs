using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Identity.Common
{
    public class FindUserResponse
    {
        public long Id { get; }
        public string EmailAddress { get; }
        public string NormalizedEmailAddress { get; }
        public string Password { get; }
        public string PasswordHash { get; }
        public bool IsEmailConfirmed { get; }

        internal FindUserResponse(User user)
        {
            Id = user.Id;
            EmailAddress = user.EmailAddress;
            NormalizedEmailAddress = user.NormalizedEmailAddress;
            Password = user.Password;
            PasswordHash = user.PasswordHash;
            IsEmailConfirmed = user.IsEmailConfirmed;
        }
    }
}

using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    public static class UserExtensions
    {
        public static UserDto ToDto(this User user)
        {
            return new UserDto(
                user.Id,
                user.EmailAddress,
                user.WorkflowType);
        }
    }
}

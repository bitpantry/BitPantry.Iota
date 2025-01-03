using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Application
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

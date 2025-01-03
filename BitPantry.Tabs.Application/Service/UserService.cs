using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Application.Service
{
    public class UserService
    {
        private EntityDataContext _dbCtx;

        public UserService(EntityDataContext dbCtx) 
        {
            _dbCtx = dbCtx;
        }

        public async Task<UserDto> GetUser(long userId)
            => (await _dbCtx.Users.AsNoTracking().SingleAsync(u => u.Id == userId)).ToDto();
    }
}

using BitPantry.CommandLine.API;
using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Console.Commands.User
{
    [Command(Namespace = "user")]
    public class Find : CommandBase
    {
        private EntityDataContext _dbCtx;

        [Argument]
        [Alias('e')]
        public string EmailAddress { get; set; }

        public Find(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task Execute(CommandExecutionContext ctx)
        {
            var user = _dbCtx.Users.AsNoTracking().Single(u => u.EmailAddress.ToUpper() == EmailAddress.ToUpper());

            if (user == null)
            {
                Info.WriteLine("User not found");
                return;
            }
            else
            {
                Info.WriteLine(user.Id);
            }
        }
    }
}

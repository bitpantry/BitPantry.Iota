using BitPantry.CommandLine.API;
using BitPantry.Iota.Application.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Shell.Commands.User
{
    [Command(Namespace = "user")]
    [Description("Creates a new user")]
    public class Create : CommandBase
    {
        private IdentityService _idSvc;

        [Argument]
        [Alias('e')]
        [Description("The email address of the user to create")]
        public string EmailAddress { get; set; }

        public Create(IdentityService idSvc)
        {
            _idSvc = idSvc;
        }

        public async Task Execute(CommandExecutionContext ctx)
        {
            var userId = await _idSvc.SignInUser(EmailAddress, ctx.CancellationToken);
            Info.WriteLine(userId);
        }
    }
}

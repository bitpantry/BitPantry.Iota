using BitPantry.CommandLine.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Shell.Commands
{
    [Command]
    public class ListCommands : CommandBase
    {
        public void Execute(CommandExecutionContext ctx)
        {
            Info.WriteLine();

            foreach (var cmd in ctx.CommandRegistry.Commands)
            {
                var namespaceString = string.IsNullOrEmpty(cmd.Namespace) ? string.Empty : $"({cmd.Namespace})";
                Info.WriteLine($"\t{cmd.Name.PadRight(15)}\t{namespaceString}");
            }

            Info.WriteLine();
        }
    }
}

//using BitPantry.CommandLine.API;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BitPantry.Tabs.Shell.Commands
//{
//    [Command]
//    public class Help : CommandBase
//    {
//        [Argument]
//        [Alias('c')]
//        public string CommandName { get; set; }

//        public void Execute(CommandExecutionContext ctx)
//        {
//            if(string.IsNullOrEmpty(CommandName))
//            {
//                Error.WriteLine("Command name is required");
//                return;
//            }

//            var nspace = CommandName.IndexOf('.') > 0 ? CommandName.Split('.')[0] : null;
//            var name = CommandName.IndexOf('.') > 0 ? CommandName.Split('.')[1] : CommandName;

//            var cmds = string.IsNullOrEmpty(nspace)
//                ? ctx.CommandRegistry.Commands.Where(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList()
//                : ctx.CommandRegistry.Commands.Where(c => c.Namespace.Equals(nspace, StringComparison.InvariantCultureIgnoreCase) && c.Name.Equals(CommandName, StringComparison.InvariantCultureIgnoreCase)).ToList();

//            if (!cmds.Any()) 
//            {
//                Error.WriteLine("Command not found");
//                return;
//            }
//            else if(cmds.Count() > 1)
//            {
//                Error.WriteLine("Multiple commands found.");
//                Error.WriteLine();
               
//                foreach (var item in cmds)
//                {
//                    var namespaceString = string.IsNullOrEmpty(item.Namespace) ? string.Empty : $"({item.Namespace})";
//                    Info.WriteLine($"\t{item.Name.PadRight(15)}\t{namespaceString}");
//                }

//                Error.WriteLine();

//                return;
//            }

//            var command = cmds.Single();

//            Info.WriteLine();

//            var spstr = string.IsNullOrEmpty(command.Namespace) ? string.Empty : $"({command.Namespace})";
//            Info.WriteLine($"\t{command.Name}\t{spstr}");
//            Info.WriteLine();

//            if (!string.IsNullOrEmpty(command.Description))
//            {
//                Info.WriteLine($"\t{command.Description}");
//                Info.WriteLine();
//            }

//            foreach (var arg in command.Arguments)
//            {
//                Info.WriteLine();
//                Info.WriteLine($"\t{arg.Name.PadRight(15)}[{arg.Alias}]\t{arg.DataType.ToString().PadRight(15)}");

//                if(!string.IsNullOrEmpty(arg.Description))
//                    Info.WriteLine($"\t\t{arg.Description}");
//            }

//            Info.WriteLine();
//        }
//    }
//}

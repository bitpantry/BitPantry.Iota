using BitPantry.CommandLine.API;
using BitPantry.Iota.Application.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Console.Commands.Util
{
    [Command(Namespace = "util")]
    public class ExportUserData : CommandBase
    {
        private ArchiveService _archSvc;

        [Argument]
        [Alias('u')]
        public long? UserId { get; set; }

        [Argument]
        [Alias('f')]
        public string OutputFilePath { get; set; }

        public ExportUserData(ArchiveService archSvc)
        {
            _archSvc = archSvc;
        }

        public async Task Execute(CommandExecutionContext ctx)
        {
            if (!UserId.HasValue && !Confirm("Export all user data?"))
                return;

            await _archSvc.ArchiveUserData(OutputFilePath, UserId, ctx.CancellationToken);

            var info = new FileInfo(OutputFilePath);

            Info.WriteLine($"{info.Length} bytes exported");

            if (Confirm("Data exported - open file?"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = OutputFilePath,
                    UseShellExecute = true
                });
            }
        }
    }
}

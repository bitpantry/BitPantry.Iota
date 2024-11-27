using BitPantry.CommandLine.API;
using BitPantry.Iota.Application.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Console.Commands.Util
{
    [Command(Namespace = "util")]
    public class ImportUserData : CommandBase
    {
        private ArchiveService _archSvc;
        private ILogger<ImportUserData> _logger;

        [Argument]
        [Alias('f')]
        public string InputFilePath { get; set; }

        [Argument]
        [Alias('r')]
        public bool RecreateCards { get; set; } = false;

        public ImportUserData(ILogger<ImportUserData> logger, ArchiveService archSvc)
        {
            _archSvc = archSvc;
            _logger = logger;
        }

        public async Task Execute(CommandExecutionContext ctx)
        {
            await _archSvc.ImportUserData(InputFilePath, RecreateCards, ctx.CancellationToken);
        }
    }
}

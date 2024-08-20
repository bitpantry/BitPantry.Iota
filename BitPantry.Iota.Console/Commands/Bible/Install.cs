using BitPantry.CommandLine.API;
using BitPantry.Iota.Application.CRQS.Bible.Command;
using BitPantry.Iota.Data.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Console.Commands.Bible
{
    [Command(Namespace = "bible")]
    public class Install : CommandBase
    {
        private IMediator _med;
        private EntityDataContext _dbCtx;

        [Argument]
        [Alias('f')]
        [Description("The Bible data file to install")]
        public string DataFilePath { get; set; }

        [Argument]
        [Alias('d')]
        [Description("A directory containing Bible data files - all files matching the pattern \"*.xml\" in the directory will be installed")]
        public string DataDirectoryPath { get; set; }

        public Install(IMediator med, EntityDataContext dbCtx)
        {
            _med = med;
            _dbCtx = dbCtx;
        }

        public async Task Execute(CommandExecutionContext context)
        {
            if(!string.IsNullOrEmpty(DataFilePath) && !string.IsNullOrEmpty(DataDirectoryPath))
            {
                Error.WriteLine("Specify only a file path or a folder path, not both.");
            }
            else if(!string.IsNullOrEmpty(DataFilePath))
            {
                if (!File.Exists(DataFilePath))
                    Error.WriteLine($"File '{DataFilePath}' not found");
                else
                    await InstallFiles(context.CancellationToken, DataFilePath);
            }
            else if(!string.IsNullOrEmpty(DataDirectoryPath)) 
            {
                if (!Directory.Exists(DataDirectoryPath))
                {
                    Error.WriteLine($"Directory '{DataDirectoryPath}' not found");
                }
                else
                {
                    var files = new DirectoryInfo(DataDirectoryPath).GetFiles("*.xml").ToArray();
                    if (files.Length == 0)
                        Error.WriteLine($"No files found in directory '{DataDirectoryPath}' matching the pattern '*.xml'");
                    else
                        await InstallFiles(context.CancellationToken, new DirectoryInfo(DataDirectoryPath).GetFiles("*.xml").Select(f => f.FullName).ToArray());
                }
            }

        }
        private async Task InstallFiles(CancellationToken cancellationToken, params string[] dataFiles)
        {
            Info.WriteLine();
            Info.WriteLine($"Installing {dataFiles.Count()} Bibles ...");
            Info.WriteLine();

            foreach (var item in dataFiles)
                Info.WriteLine($"\t{item}");

            Info.WriteLine();

            if (Confirm("Continue?"))
            {
                Info.WriteLine();

                foreach (var item in dataFiles)
                {
                    if (dataFiles.Count() > 1)
                        Info.WriteLine($"Installing '{item}' ...");

                    await _med.Send(new InstallBibleCommand(item), cancellationToken);
                }
            }
        }
    }
}

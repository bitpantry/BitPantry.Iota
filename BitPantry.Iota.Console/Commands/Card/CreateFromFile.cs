using BitPantry.CommandLine.API;
using BitPantry.Iota.Application.CRQS.Bible.Query;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Console.Commands.Card
{
    [Command(Namespace = "card")]
    public class CreateFromFile : CommandBase
    {
        private EntityDataContext _dbCtx;
        private IMediator _med;

        [Argument]
        [Alias('u')]
        [Description("The id of the user to create the card for")]
        public long UserId { get; set; }

        [Argument]
        [Alias('b')]
        [Description("The id of the Bible to use when creating the card. If not specified, the first bible (by Bible.Id) will be used.")]
        public long BibleId { get; set; }

        [Argument]
        [Alias('f')]
        [Description("The path of the file to load. Each line should contain a single verse address (blank lines are ignored).")]
        public string FilePath { get; set; }

        public CreateFromFile(EntityDataContext dbCtx, IMediator med) 
        {
            _dbCtx = dbCtx;
            _med = med;
        }

        public async Task Execute(CommandExecutionContext ctx)
        {
            var isError = false;

            if (UserId == 0)
            {
                Error.WriteLine("UserId is required");
                isError = true;
            }

            if (BibleId == 0)
            {
                var biblesResp = await _med.Send(new GetBibleTranslationsQuery());
                var firstTranslation = biblesResp.First();
                Info.WriteLine($"Using translation {firstTranslation.LongName}");
                BibleId = firstTranslation.Id;
            }

            if(!File.Exists(FilePath))
            {
                Error.WriteLine($"File not found at '{FilePath}'");
                isError = true;
            }

            if (isError)
                return;

            foreach (var address in File.ReadLines(FilePath))
            {
                if(!string.IsNullOrEmpty(address))
                {
                    var resp = await _med.Send(new CreateCardCommand(UserId, BibleId, address));

                    if (!resp.IsValidAddress)
                    {
                        Warning.WriteLine($"No passage found for address, '{address}'", address);
                    }
                    else if (resp.isAlreadyCreated)
                    {
                        Warning.WriteLine($"Card for passage, '{resp.Address}', already exists");
                    }
                }
            }

        }
    }
}

using BitPantry.CommandLine.API;
using BitPantry.Tabs.Application.Parsers;
using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Data.Entity;

namespace BitPantry.Tabs.Shell.Commands.Card
{
    [Command(Namespace = "card")]
    public class CreateFromFile : CommandBase
    {
        private readonly BibleService _bibleSvc;
        private readonly CardService _cardSvc;

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

        public CreateFromFile(BibleService bibleSvc, CardService cardSvc) 
        {
            _bibleSvc = bibleSvc;
            _cardSvc = cardSvc;
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
                var biblesResp = await _bibleSvc.GetBibleTranslations(ctx.CancellationToken);
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
                    try
                    {
                        var resp = await _cardSvc.CreateCard(UserId, BibleId, address, ctx.CancellationToken);
                    }
                    catch (CreateCardException createCardEx)
                    {
                        Error.WriteLine(createCardEx.Message);
                    }
                    catch (PassageAddressParsingException parsingEx)
                    {
                        Error.WriteLine(parsingEx.Message);
                    }
                }
            }

        }
    }
}

//using BitPantry.CommandLine.API;
//using BitPantry.Tabs.Application.Parsers;
//using BitPantry.Tabs.Application.Service;
//using BitPantry.Tabs.Common;

//namespace BitPantry.Tabs.Shell.Commands.Card
//{
//    [Command(Namespace = "Card")]
//    public class Create : CommandBase
//    {
//        private readonly BibleService _bibleSvc;
//        private readonly CardService _cardSvc;

//        [Argument]
//        [Alias('u')]
//        [Description("The id of the user to create the card for")]
//        public long UserId { get; set; }

//        [Argument]
//        [Alias('b')]
//        [Description("The id of the Bible to use when creating the card.")]
//        public long BibleId { get; set; }

//        [Argument]
//        [Alias('a')]
//        [Description("The address of the passage to create a card for.")]
//        public string Address { get; set; }

//        [Argument]
//        [Alias('d')]
//        [Description("The tab to put the new card into - Queue by default.")]
//        public Tab Tab { get; set; } = Tab.Queue;

//        public Create(BibleService bibleSvc, CardService cardSvc)
//        {
//            _bibleSvc = bibleSvc;
//            _cardSvc = cardSvc;
//        }

//        public async Task Execute(CommandExecutionContext context)
//        {
//            var isError = false;

//            if (UserId == 0)
//            {
//                Error.WriteLine("UserId is required");
//                isError = true;
//            }

//            if (BibleId == 0)
//            {
//                var biblesResp = await _bibleSvc.GetBibleTranslations(context.CancellationToken);
//                var firstTranslation = biblesResp.First();
//                Info.WriteLine($"Using translation {firstTranslation.LongName}");
//                BibleId = firstTranslation.Id;
//            }

//            if (string.IsNullOrEmpty(Address))
//            {
//                Error.WriteLine("Address is required");
//                isError = true;
//            }

//            if (isError)
//                return;

//            try
//            {
//                var resp = await _cardSvc.CreateCard(UserId, BibleId, Address, Tab, context.CancellationToken);
//            }
//            catch(CreateCardException createCardEx)
//            {
//                Error.WriteLine(createCardEx.Message);
//            }
//            catch(PassageAddressParsingException parsingEx)
//            {
//                Error.WriteLine(parsingEx.Message);
//            }

//        }
//    }
//}

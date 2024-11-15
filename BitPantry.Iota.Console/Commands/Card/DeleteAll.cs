using BitPantry.CommandLine.API;
using BitPantry.Iota.Application.Service;

namespace BitPantry.Iota.Console.Commands.Card
{
    [Command(Namespace = "Card")]
    public class DeleteAll : CommandBase
    {
        private readonly CardService _cardSvc;

        public DeleteAll(CardService cardSvc)
        {
            _cardSvc = cardSvc;
        }

        public async Task Execute(CommandExecutionContext context)
        {
            if (Confirm("All cards will be deleted"))
                await _cardSvc.DeleteAllCards(null, context.CancellationToken);

        }
    }
}


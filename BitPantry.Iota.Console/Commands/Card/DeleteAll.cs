using BitPantry.CommandLine.API;
using BitPantry.Iota.Application.Service;
using System.Net.Sockets;

namespace BitPantry.Iota.Console.Commands.Card
{
    [Command(Namespace = "Card")]
    public class DeleteAll : CommandBase
    {
        private readonly CardService _cardSvc;

        [Argument]
        [Alias('u')]
        public long UserId { get; set; }

        public DeleteAll(CardService cardSvc)
        {
            _cardSvc = cardSvc;
        }

        public async Task Execute(CommandExecutionContext context)
        {
            if (UserId > 0)
            {
                if (Confirm($"All cards will be deleted for user {UserId}"))
                    await _cardSvc.DeleteAllCards(UserId, context.CancellationToken);
            }
            else
            {
                if (Confirm("All cards will be deleted"))
                    await _cardSvc.DeleteAllCards(null, context.CancellationToken);
            }

        }
    }
}


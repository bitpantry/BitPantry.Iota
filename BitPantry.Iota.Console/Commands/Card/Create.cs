using BitPantry.CommandLine.API;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Console.Commands.Card
{
    [Command(Namespace = "Card")]
    public class Create : CommandBase
    {
        private IMediator _med;

        [Argument]
        [Alias('u')]
        [Description("The id of the user to create the card for")]
        public long UserId { get; set; }

        [Argument]
        [Alias('b')]
        [Description("The id of the Bible to use when creating the card.")]
        public long BibleId { get; set; }

        [Argument]
        [Alias('a')]
        [Description("The address of the passage to create a card for.")]
        public string Address { get; set; }

        [Argument]
        [Alias('d')]
        [Description("The divider to put the new card into - Queue by default.")]
        public Divider Divider { get; set; } = Divider.Queue;

        public Create(IMediator med)
        {
            _med = med;
        }

        public async Task Execute(CommandExecutionContext context)
        {
            var isError = false;

            if (UserId == 0)
            {
                Error.WriteLine("UserId is required");
                isError = true;
            }

            if (BibleId == 0)
            {
                Error.WriteLine("BibleId is required");
                isError = true;
            }

            if (string.IsNullOrEmpty(Address))
            {
                Error.WriteLine("Address is required");
                isError = true;
            }

            if (isError)
                return;

            var resp = await _med.Send(new Application.CRQS.Card.Command.CreateCardCommand(UserId, BibleId, Address, Divider));

            if(!resp.IsValidAddress) {
                Error.WriteLine("Invalid address");

                if (resp.isAlreadyCreated)
                    Error.WriteLine("Card already exists");
            }
        }
    }
}

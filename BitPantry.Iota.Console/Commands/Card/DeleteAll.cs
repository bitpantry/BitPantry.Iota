using BitPantry.CommandLine.API;
using BitPantry.Iota.Application.CRQS.Card.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Console.Commands.Card
{
    [Command(Namespace = "Card")]
    public class DeleteAll : CommandBase
    {
        private IMediator _med;

        public DeleteAll(IMediator med)
        {
            _med = med;
        }

        public async Task Execute(CommandExecutionContext context)
        {
            if (Confirm("All cards will be deleted"))
                await _med.Send(new DeleteAllCardsCommand());

        }
    }
}


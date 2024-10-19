using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using BitPantry.Iota.Common;
using BitPantry.Iota.Application.Service;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class ReorderCardCommandHandler : IRequestHandler<ReorderCardCommand>
    {
        private CardService _cardSvc;

        public ReorderCardCommandHandler(CardService cardSvc)
        {
            _cardSvc = cardSvc;
        }

        public async Task Handle(ReorderCardCommand request, CancellationToken cancellationToken)
        {
            await _cardSvc.ReorderCard(request.UserId, request.CardId, request.Divider, request.NewOrder);
        }
    }

    public record ReorderCardCommand(Divider Divider, long UserId, long CardId, int NewOrder) : IRequest { }
}

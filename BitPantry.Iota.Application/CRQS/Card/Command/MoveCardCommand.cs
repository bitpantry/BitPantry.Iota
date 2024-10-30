using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Azure.Core;
using System.Data.Common;
using BitPantry.Iota.Application.Service;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class MoveCardCommandHandler : IRequestHandler<MoveCardCommand>
    {
        private readonly CardService _cardSvc;

        public MoveCardCommandHandler(CardService cardSvc)
        {
            _cardSvc = cardSvc;
        }

        public async Task Handle(MoveCardCommand request, CancellationToken cancellationToken)
        {
            await _cardSvc.MoveCard(request.CardId, request.NewTab);
        }
    }

    public record MoveCardCommand(long CardId, Tab NewTab) : IRequest { } 
}

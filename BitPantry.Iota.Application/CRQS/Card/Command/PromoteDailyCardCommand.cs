using BitPantry.Iota.Application.Service;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class PromoteDailyCardCommandHandler : IRequestHandler<PromoteDailyCardCommand>
    {
        private CardService _cardSvc;

        public PromoteDailyCardCommandHandler(CardService cardSvc)
        {
            _cardSvc = cardSvc;
        }

        public async Task Handle(PromoteDailyCardCommand request, CancellationToken cancellationToken)
        {
            await _cardSvc.PromoteDailyCard(request.cardId);
        }
    }

    public record PromoteDailyCardCommand(long cardId) : IRequest { }
}

using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Data.Entity;
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
        private EntityDataContext _dbCtx;
        private CardService _cardSvc;
        private ReviewService _reviewSvc;

        public PromoteDailyCardCommandHandler(EntityDataContext dbCtx, CardService cardSvc, ReviewService reviewSvc)
        {
            _dbCtx = dbCtx;
            _cardSvc = cardSvc;
            _reviewSvc = reviewSvc;
        }

        public async Task Handle(PromoteDailyCardCommand request, CancellationToken cancellationToken)
        {
            await _cardSvc.PromoteDailyCard(request.cardId);

            // make sure the promoted daily card is removed from the current review session

            var session = await _reviewSvc.GetReviewSession(_dbCtx, request.userId);
            session.Item1.AddCardToIgnore(request.cardId);

            await _dbCtx.SaveChangesAsync();
        }
    }

    public record PromoteDailyCardCommand(long userId, long cardId) : IRequest { }
}

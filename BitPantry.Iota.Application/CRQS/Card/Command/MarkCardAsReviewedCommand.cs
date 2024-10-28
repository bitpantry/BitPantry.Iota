using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class MarkCardAsReviewedCommandHandler : IRequestHandler<MarkCardAsReviewedCommand>
    {
        private EntityDataContext _dbCtx;

        public MarkCardAsReviewedCommandHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        async Task IRequestHandler<MarkCardAsReviewedCommand>.Handle(MarkCardAsReviewedCommand request, CancellationToken cancellationToken)
        {
            var card = await _dbCtx.Cards
                .Where(c => c.UserId == request.UserId)
                .Where(c => c.Divider == request.CardDivider)
                .Where(c => c.Order == request.CardOrder)
                .SingleAsync();

            card.LastReviewedOn = DateTime.UtcNow;

            await _dbCtx.SaveChangesAsync();
        }
    }

    public record MarkCardAsReviewedCommand(long UserId, Divider CardDivider, int CardOrder) : IRequest
    {
    }
}

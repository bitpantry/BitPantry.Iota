using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class MarkCardAsReviewedCommandHandler : IRequestHandler<MarkCardAsReviewedCommand>
    {
        private ILogger<MarkCardAsReviewedCommandHandler> _logger;
        private EntityDataContext _dbCtx;

        public MarkCardAsReviewedCommandHandler(ILogger<MarkCardAsReviewedCommandHandler> logger, EntityDataContext dbCtx)
        {
            _logger = logger;
            _dbCtx = dbCtx;
        }

        async Task IRequestHandler<MarkCardAsReviewedCommand>.Handle(MarkCardAsReviewedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Marking card {CardTab}:{CardOrder} as reviewed", request.CardTab, request.CardOrder);

            var card = await _dbCtx.Cards
                .Where(c => c.UserId == request.UserId)
                .Where(c => c.Tab == request.CardTab)
                .Where(c => c.Order == request.CardOrder)
                .SingleAsync();

            card.LastReviewedOn = DateTime.UtcNow;

            await _dbCtx.SaveChangesAsync(cancellationToken);
        }
    }

    public record MarkCardAsReviewedCommand(long UserId, Tab CardTab, int CardOrder) : IRequest
    {
    }
}

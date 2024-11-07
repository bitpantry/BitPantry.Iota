using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Card.Command
{
    public class PromoteDailyCardCommandHandler : IRequestHandler<PromoteDailyCardCommand>
    {
        private ILogger<PromoteDailyCardCommand> _logger;
        private EntityDataContext _dbCtx;
        private CardLogic _cardLgc;
        private ReviewLogic _reviewLgc;

        public PromoteDailyCardCommandHandler(ILogger<PromoteDailyCardCommand> logger, EntityDataContext dbCtx, CardLogic cardLgc, ReviewLogic reviewLgc)
        {
            _logger = logger;
            _dbCtx = dbCtx;
            _cardLgc = cardLgc;
            _reviewLgc = reviewLgc;
        }

        public async Task Handle(PromoteDailyCardCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Promoting daily card {CardId}", request.cardId);

            // can this card be promoted - is it in the daily tab?

            var cardInfo = await _dbCtx.UseConnection(async (conn) =>
            {
                return await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT UserId, Tab FROM Cards WHERE Id = @CardId",
                new { request.cardId });
            });

            long userId = cardInfo.UserId;
            Tab tab = (Tab)cardInfo.Tab;

            if (tab != Tab.Daily)
                throw new InvalidOperationException($"Only cards in the {Tab.Daily} tab can be promoted. This card is in the {(Tab)tab} tab.");

            await _dbCtx.UseConnection(async (conn, trans) =>
            {
                // set the current card's last reviewed timestamp

                await conn.ExecuteAsync(
                    "UPDATE Cards SET LastReviewedOn = @Timestamp WHERE Id = @CardId",
                    new { Timestamp = DateTime.UtcNow, CardId = request.cardId },
                    transaction: trans);

                // try to promote the queue card if one exists

                if (!await _cardLgc.TryPromoteNextQueueCardCommand(conn, trans, userId))
                {
                    var promotionTab = await _cardLgc.GetPromotionTabQuery(conn, trans, userId, tab);
                    await _cardLgc.MoveCardCommand(conn, trans, request.cardId, promotionTab);
                }
            });

            // make sure the promoted daily card is removed from the current review session

            var session = await _reviewLgc.GetReviewSessionCommand(_dbCtx, request.userId);
            session.Item1.AddCardToIgnore(request.cardId);
        }
    }

    public record PromoteDailyCardCommand(long userId, long cardId) : IRequest { }
}

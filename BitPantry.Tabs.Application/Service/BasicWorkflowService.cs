using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Application.Logic;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Application.Service
{
    public class BasicWorkflowService : IWorkflowService
    {
        private readonly ILogger<BasicWorkflowService> _logger;
        private readonly EntityDataContext _dbCtx;
        private readonly CardService _cardSvc;
        private readonly CardPromotionLogic _cardPromotionLgc;

        public BasicWorkflowService(ILogger<BasicWorkflowService> logger, EntityDataContext dbCtx, CardService cardSvc, CardPromotionLogic cardPromotionLgc)
        {
            _logger = logger;
            _dbCtx = dbCtx;
            _cardSvc = cardSvc;
            _cardPromotionLgc = cardPromotionLgc;
        }

        public async Task<ReviewPathDto> GetReviewPath(long userId, DateTime userLocalTime, CancellationToken cancellationToken)
        {
            var path = new Dictionary<Tab, int>();
            var currentTab = Tab.Queue;

            do
            {
                currentTab = currentTab.GetNextReviewTab(userLocalTime);
                path.Add(currentTab, 0);

            } while (currentTab < Tab.Day1);

            // select the card count grouped by tab for the userId

            var cardCounts = await _dbCtx.Cards
                .AsNoTracking()
                .Where(card => card.UserId == userId)
                .GroupBy(card => card.Tab)
                .Select(group => new
                {
                    Tab = group.Key,
                    CardCount = group.Count()
                })
                .ToListAsync(cancellationToken);

            foreach (var count in cardCounts)
            {
                if (path.ContainsKey(count.Tab))
                    path[count.Tab] = count.CardCount;
            };

            return new ReviewPathDto(
                userId,
                new Dictionary<Tab, int>(path));
        }

        public async Task PromoteCard(long cardId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Promoting card {CardId}", cardId);

            // get basic card info

            var cardInfo = await _dbCtx.UseConnection(cancellationToken, async (conn) =>
            {
                return await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT UserId, Tab FROM Cards WHERE Id = @CardId",
                new { cardId });
            });

            long userId = cardInfo.UserId;
            Tab tab = (Tab)cardInfo.Tab;

            // can this card be promoted - is it in the daily tab?

            if (tab != Tab.Daily)
                throw new InvalidOperationException($"Only cards in the {Tab.Daily} tab can be promoted. This card is in the {tab} tab.");

            // promote the daily card

            var promotionTab = await _dbCtx.UseConnection(cancellationToken, async conn =>
                await _cardPromotionLgc.GetPromotionTabQuery(conn, userId, tab, WorkflowType.Basic));

            await MoveCard_INTERNAL(userId, cardId, tab, promotionTab, false, cancellationToken);  

        }

        public async Task MoveCard(long cardId, Tab toTab, bool toTop, CancellationToken cancellationToken)
        {
            var cardInfo = await _dbCtx.UseConnection(cancellationToken, async (conn) =>
            {
                return await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT UserId, Tab FROM Cards WHERE Id = @CardId",
                new { cardId });
            });

            long userId = cardInfo.UserId;
            Tab tab = (Tab)cardInfo.Tab;

            await MoveCard_INTERNAL(userId, cardId, tab, toTab, toTop, cancellationToken);
        }

        private async Task MoveCard_INTERNAL(long userId, long cardId, Tab fromTab, Tab toTab, bool toTop, CancellationToken cancellationToken)
        {
            // move the card

            await _cardSvc.MoveCard(cardId, toTab, toTop, cancellationToken);

            // cascade using basic workflow constraints / rules - if the tab can only have one card, then recursively push out

            if (toTab > Tab.Queue && toTab < Tab.Day1)
            {
                var existingCardId = await _dbCtx.UseConnection(cancellationToken, async conn =>
                {
                    return await conn.QuerySingleOrDefaultAsync<long?>(
                    @"SELECT Id
                      FROM Cards
                      WHERE UserId = @UserId
                      AND Tab = @ToTab
                      AND Id != @CardId",
                    new { UserId = userId, ToTab = toTab, CardId = cardId });
                });

                if (existingCardId.HasValue)
                {
                    var promotionTab = await _dbCtx.UseConnection(cancellationToken, async conn =>
                        await _cardPromotionLgc.GetPromotionTabQuery(conn, userId, toTab, WorkflowType.Basic));

                    _logger.LogDebug("Bumping card {CardId}", existingCardId.Value);

                    await MoveCard_INTERNAL(userId, existingCardId.Value, toTab, promotionTab, false, cancellationToken);
                }
            }
        }

        public async Task DeleteCard(long cardId, CancellationToken cancellationToken)
        { 
            // get card tab

            var tabInt = await _dbCtx.UseConnection(cancellationToken, async conn => await conn.QuerySingleOrDefaultAsync<int?>("SELECT Tab FROM Cards WHERE Id = @Id", new { Id = cardId }));

            if (!tabInt.HasValue)
                throw new InvalidOperationException($"The card, {cardId}, does not exist");

            // delete the card

            await _cardSvc.DeleteCard(cardId, cancellationToken);

        }

        public async Task StartQueueCard(long queueCardId, CancellationToken cancellationToken)
        {
            // get the queue card

            var queueCard = await _dbCtx.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == queueCardId, cancellationToken);

            if (queueCard.Tab != Tab.Queue)
                throw new InvalidOperationException($"Card with id {queueCardId} is expected to be in the queue");

            // get the daily card and make updates

            var dailyCard = await _dbCtx.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == queueCard.UserId && c.Tab == Tab.Daily);

            // swap

            _logger.LogDebug("Swapping top queue card {QueueCardId} with daily card {DailyCardId}", queueCard.Id, dailyCard == null ? 0 : dailyCard.Id);

            await _cardSvc.MoveCard(queueCardId, Tab.Daily, true, cancellationToken);
            if (dailyCard != null)
                await _cardSvc.MoveCard(dailyCard.Id, Tab.Queue, true, cancellationToken);
        }
    }
}

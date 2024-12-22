using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
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

namespace BitPantry.Iota.Application.Service
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
                currentTab = GetNextReviewTab(currentTab, userLocalTime);
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
                new Dictionary<Tab, int>(path.Where(p => p.Value > 0)));
        }

        private Tab GetNextReviewTab(Tab? lastTab, DateTime userLocalTime) => lastTab switch
        {
            Tab.Queue => Tab.Daily,
            Tab.Daily => userLocalTime.Day % 2 == 0 ? Tab.Even : Tab.Odd,
            Tab.Odd or Tab.Even => userLocalTime.DayOfWeek switch
            {
                DayOfWeek.Sunday => Tab.Sunday,
                DayOfWeek.Monday => Tab.Monday,
                DayOfWeek.Tuesday => Tab.Tuesday,
                DayOfWeek.Wednesday => Tab.Wednesday,
                DayOfWeek.Thursday => Tab.Thursday,
                DayOfWeek.Friday => Tab.Friday,
                DayOfWeek.Saturday => Tab.Saturday,
                _ => throw new ArgumentOutOfRangeException("DateTime.Today.DayOfWeek", userLocalTime.DayOfWeek, "A tab is not defined for this day of the week")
            },
            Tab.Sunday or Tab.Monday or Tab.Tuesday or Tab.Wednesday or Tab.Thursday or Tab.Friday or Tab.Saturday => userLocalTime.Day + Tab.Saturday,
            _ => throw new ArgumentOutOfRangeException(nameof(lastTab), lastTab.Value, "No review path is defined for this tab")
        };

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

            // if the card being moved is the daily card and there is a different card in the queue, move up the next queue card (if any)

            if (tab == Tab.Daily)
            {
                var nextQueueCardId = await _dbCtx.UseConnection(cancellationToken, async conn =>
                {
                    return await conn.QuerySingleOrDefaultAsync<long?>(
                    "SELECT TOP 1 Id FROM Cards WHERE UserId = @UserId AND Tab = @QueueTab ORDER BY [Order]",
                    new
                    {
                        UserId = userId,
                        QueueTab = Tab.Queue
                    });
                });

                if (nextQueueCardId.HasValue && nextQueueCardId != cardId)
                    await MoveCard_INTERNAL(userId, nextQueueCardId.Value, Tab.Queue, Tab.Daily, toTop = true, cancellationToken);
            }
        }

        private async Task MoveCard_INTERNAL(long userId, long cardId, Tab fromTab, Tab toTab, bool toTop, CancellationToken cancellationToken)
        {
            // move the card

            await _cardSvc.MoveCard(cardId, toTab, cancellationToken, toTop);

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

            // if card is daily, check for next available queue card

            long? nextQueueCardId = null;

            if (tabInt.Value == (int)Tab.Daily)
            {
                nextQueueCardId = await _dbCtx.UseConnection(cancellationToken, async conn =>
                {
                    return await conn.QuerySingleOrDefaultAsync<long?>(
                    "SELECT TOP 1 Id FROM Cards WHERE UserId = (SELECT UserId FROM Cards WHERE Id = @CardId) AND Tab = @QueueTab ORDER BY [Order]",
                    new
                    {
                        CardId = cardId,
                        QueueTab = Tab.Queue
                    });
                });
            }

            // delete the card

            await _cardSvc.DeleteCard(cardId, cancellationToken);

            // if the card was in the daily tab, promote the next queued card if available

            if(nextQueueCardId.HasValue)
                await MoveCard(nextQueueCardId.Value, Tab.Daily, true, cancellationToken);
        }

        public async Task SwapTopQueueCardForDaily(long queueCardId, CancellationToken cancellationToken)
        {
            // get the queue card

            var queueCard = await _dbCtx.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == queueCardId, cancellationToken);

            if (queueCard.Tab != Tab.Queue)
                throw new InvalidOperationException($"Card with id {queueCardId} is expected to be in the queue");

            // get the daily card and make updates

            var dailyCard = await _dbCtx.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == queueCard.UserId && c.Tab == Tab.Daily);

            // swap

            _logger.LogDebug("Swapping top queue card {QueueCardId} with daily card {DailyCardId}", queueCard.Id, dailyCard == null ? 0 : dailyCard.Id);

            await _cardSvc.MoveCard(queueCardId, Tab.Daily, cancellationToken);
            if (dailyCard != null)
                await _cardSvc.MoveCard(dailyCard.Id, Tab.Queue, cancellationToken);
        }
    }
}

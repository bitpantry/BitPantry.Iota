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
    public class AdvancedWorkflowService : IWorkflowService
    {
        private readonly ILogger<AdvancedWorkflowService> _logger;
        private readonly EntityDataContext _dbCtx;
        private readonly CardService _cardSvc;
        private readonly CardPromotionLogic _cardPromotionLgc;

        public AdvancedWorkflowService(ILogger<AdvancedWorkflowService> logger, EntityDataContext dbCtx, CardService cardSvc, CardPromotionLogic cardPromotionLgc)
        {
            _logger = logger;
            _dbCtx = dbCtx;
            _cardSvc = cardSvc;
            _cardPromotionLgc = cardPromotionLgc;
        }

        public async Task DeleteCard(long cardId, CancellationToken cancellationToken)
        {
            var card = await _dbCtx.UseConnection(cancellationToken, async conn => await conn.QueryFirstOrDefaultAsync("SELECT UserId, Tab FROM Cards WHERE Id = @Id", new { Id = cardId }));

            if (card == null)
                throw new InvalidOperationException($"The card, {cardId}, does not exist");

            long userId = card.UserId;
            Tab tab = (Tab)card.Tab;

            // delete the card

            await _cardSvc.DeleteCard(cardId, cancellationToken);

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

        public async Task MoveCard(long cardId, Tab toTab, bool toTop, CancellationToken cancellationToken)
        {
            await _cardSvc.MoveCard(cardId, toTab, toTop, cancellationToken);
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

            // promote the daily card

            var promotionTab = await _dbCtx.UseConnection(cancellationToken, async conn =>
                await _cardPromotionLgc.GetPromotionTabQuery(conn, userId, tab, WorkflowType.Advanced));

            await MoveCard(cardId, promotionTab, false, cancellationToken);
        }

        public async Task StartQueueCard(long queueCardId, CancellationToken cancellationToken)
        {
            // get the queue card

            var queueCard = await _dbCtx.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == queueCardId, cancellationToken);

            if (queueCard.Tab != Tab.Queue)
                throw new InvalidOperationException($"Card with id {queueCardId} is expected to be in the queue");

            // start card

            await _cardSvc.MoveCard(queueCardId, Tab.Daily, true, cancellationToken);
        }
    }
}

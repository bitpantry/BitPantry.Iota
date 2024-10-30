using Azure.Core;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public class CardService
    {
        private DbConnectionFactory _db;
        private ILogger<CardService> _logger;

        public CardService(DbConnectionFactory db, ILogger<CardService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<GetCardResult> GetCard(EntityDataContext dbCtx, long cardId)
        {
            var card = await GetCardQuery(dbCtx)
                .Where(c => c.Id == cardId)
                .FirstOrDefaultAsync();

            return BuildGetCardResult(card);

        }

        public async Task<GetCardResult> GetCard(EntityDataContext dbCtx, long userId, Tab tab, int cardOrder)
        {
            // get card data, including verses

            var card = await GetCardQuery(dbCtx)
                .Where(c => c.Tab == tab && c.Order == cardOrder && c.UserId == userId)
                .FirstOrDefaultAsync();

            return BuildGetCardResult(card);
        }

        public async Task<List<GetCardResult>> GetCards(EntityDataContext dbCtx, long userId, Tab tab)
        {
            var cards = await GetCardQuery(dbCtx)
                .Where(c => c.Tab == tab && c.UserId == userId)
                .ToListAsync();

            return cards.Select(BuildGetCardResult).ToList();
        }

        private IQueryable<Card> GetCardQuery(EntityDataContext dbCtx)
        {
            return dbCtx.Cards
                .AsNoTracking()
                .Include(c => c.Verses)
                .ThenInclude(v => v.Chapter)
                .ThenInclude(c => c.Book)
                .ThenInclude(b => b.Testament)
                .ThenInclude(t => t.Bible);
        }

        private GetCardResult BuildGetCardResult(Card card)
        {
            card.Verses = [.. card.Verses.OrderBy(v => v.Chapter.Number).ThenBy(v => v.Number)];

            // return result

            return new GetCardResult(
                card.Id,
                card.AddedOn,
                card.LastMovedOn,
                card.LastReviewedOn,
                card.Tab,
                card.Order,
                card.Verses);
        }

        public async Task PromoteDailyCard(long cardId)
        {
            _logger.LogDebug("Promoting daily card {CardId}", cardId);

            var dbConnection = _db.GetDbConnection();

            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();

            // can this card be promoted - is it in the daily tab?

            var cardInfo = await _db.GetDbConnection().QuerySingleOrDefaultAsync<dynamic>(
                "SELECT UserId, Tab FROM Cards WHERE Id = @CardId",
                new { cardId });

            long userId = cardInfo.UserId;
            Tab div = (Tab)cardInfo.Tab;

            if (div != Tab.Daily)
                throw new InvalidOperationException($"Only cards in the {Tab.Daily} tab can be promoted. This card is in the {(Tab)div} tab.");

            // begin a transaction

            var transaction = dbConnection.BeginTransaction();

            try
            {
                // set the current card's last reviewed timestamp

                await _db.GetDbConnection().ExecuteAsync(
                    "UPDATE Cards SET LastReviewedOn = @Timestamp WHERE Id = @CardId",
                    new { Timestamp = DateTime.UtcNow, CardId = cardId },
                    transaction: transaction);

                // try to promote the queue card if one exists

                if (!await PromoteNextQueueCard(userId, dbConnection, transaction))
                {
                    var promotionTab = await GetPromotionTab(dbConnection, transaction, userId, div);
                    await MoveCard_RECURSIVE(dbConnection, transaction, cardId, promotionTab);
                }

                transaction.Commit();

            }
            catch
            {
                transaction.Rollback();
                throw;
            }

        }

        //public async Task<bool> PromoteNextQueueCard(long userId)
        //{
        //    var dbConnection = _db.GetDbConnection();

        //    if (dbConnection.State != ConnectionState.Open)
        //        dbConnection.Open();

        //    using (var transaction = dbConnection.BeginTransaction())
        //    {
        //        try
        //        {
        //            var result = await PromoteNextQueueCard(userId, dbConnection, transaction);
        //            transaction.Commit();
        //            return result;
        //        }
        //        catch
        //        {
        //            transaction.Rollback();
        //            throw;
        //        }
        //    }
        //}

        public async Task<bool> PromoteNextQueueCard(long userId, DbConnection dbConnection, DbTransaction transaction)
        {
            // is there a card in the queue? If yes, move to daily and return true, otherwise return false

            var cardId = await dbConnection.QuerySingleOrDefaultAsync<long?>(
                "SELECT TOP 1 Id FROM Cards WHERE UserId = @UserId AND Tab = @Tab ORDER BY [Order]",
                new { userId, Tab = Tab.Queue }, transaction: transaction);

            if (cardId == null)
                return false;

            await MoveCard_RECURSIVE(dbConnection, transaction, cardId.Value, Tab.Daily);

            return true;
        }

        public async Task MoveCard(long cardId, Tab toTab)
        {
            var dbConnection = _db.GetDbConnection();

            if (dbConnection.State == System.Data.ConnectionState.Closed)
                dbConnection.Open();

            using (var transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    await MoveCard_RECURSIVE(dbConnection, transaction, cardId, toTab);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private async Task MoveCard_RECURSIVE(DbConnection dbConnection, DbTransaction transaction, long cardId, Tab toTab)
        {
            // Get the current order and tab of the card to be moved
            var cardInfo = await dbConnection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT UserId, [Order], Tab FROM Cards WHERE Id = @CardId",
                new { cardId },
                transaction: transaction);

            if (cardInfo == null)
                throw new Exception("Card not found.");

            int currentOrder = cardInfo.Order;
            int currentTab = cardInfo.Tab;
            long userId = cardInfo.UserId;

            _logger.LogDebug("Moving card {CardId} :: {CurrentTab} => {ToTab}", cardId, (Tab)currentTab, toTab);

            // Update the tab of the card
            await dbConnection.ExecuteAsync(
                "UPDATE Cards SET Tab = @ToTab, LastMovedOn = @Timestamp WHERE Id = @CardId",
            new { toTab, cardId, Timestamp = DateTime.UtcNow },
                transaction: transaction);

            // Update the order of the remaining cards within the original tab
            await dbConnection.ExecuteAsync(
                "UPDATE Cards SET [Order] = [Order] - 1 WHERE Tab = @CurrentTab AND UserId = @UserId AND [Order] > @CurrentOrder",
                new { CurrentTab = currentTab, UserId = userId, CurrentOrder = currentOrder },
                transaction: transaction);

            // Cascade promote cards in the destination tab if single card tab
            if(toTab < Tab.Day1 && toTab != Tab.Queue)
            {
                var existingCardId = await dbConnection.QuerySingleOrDefaultAsync<long?>(
                    @"SELECT Id
                      FROM Cards
                      WHERE UserId = @UserId
                      AND Tab = @ToTab
                      AND Id != @CardId",
                    new { UserId = userId, ToTab = toTab, CardId = cardId },
                    transaction: transaction);

                if (existingCardId != null)
                {
                    var promotionTab = await GetPromotionTab(dbConnection, transaction, userId, toTab);
                    _logger.LogDebug("Bumping card {CardId}", existingCardId);
                    await MoveCard_RECURSIVE(dbConnection, transaction, existingCardId.Value, promotionTab);
                }
            }
            else // put at the top of the list in any of the multi-card tab
            {
                await ReorderCard_INTERNAL(dbConnection, transaction, userId, cardId, toTab, 1);
            }
        }

        private async Task<Tab> GetPromotionTab(DbConnection dbConnection, DbTransaction transaction, long userId, Tab currentTab)
        {
            switch (currentTab)
            {
                case Tab.Queue:

                    return Tab.Daily;

                case Tab.Daily:

                    return await GetSingleCardTabOfOldestCard(dbConnection, transaction, userId, Tab.Odd, Tab.Even);

                case Tab.Odd:
                case Tab.Even:

                    return await GetSingleCardTabOfOldestCard(dbConnection, transaction, userId, Tab.Sunday, Tab.Saturday);

                case Tab.Sunday:
                case Tab.Monday:
                case Tab.Tuesday:
                case Tab.Wednesday:
                case Tab.Thursday:
                case Tab.Friday:
                case Tab.Saturday:

                    return await GetMultipleCardTabOfLeastAndOldestCards(dbConnection, transaction, userId, Tab.Day1, Tab.Day31);

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentTab), currentTab, "No promotion path is defined for this tab");
            }
        }

        private async Task<Tab> GetSingleCardTabOfOldestCard(DbConnection dbConnection, DbTransaction transaction, long userId, Tab start, Tab end)
        {

            // return the first empty tab if any

            var usedTabs = await dbConnection.QueryAsync<Tab>(
                @"SELECT DISTINCT Tab
                    FROM Cards
                    WHERE UserId = @UserId
                    AND Tab >= @Start
                    AND Tab <= @End",
                new
                {
                    UserId = userId,
                    Start = start,
                    End = end
                }, transaction);

            var unusedTabs = Enumerable.Range((int)start, (int)end - (int)start + 1).Select(i => (Tab)i)
                .Except(usedTabs)
                .Order()
                .ToList();

            if (unusedTabs.Count != 0)
                return unusedTabs.First();

            // return the tab with the oldest moved date

            var div = await dbConnection.QuerySingleAsync<Tab>(
                @"SELECT TOP 1 Tab
                    FROM Cards
                    WHERE UserId = @UserId
                    AND Tab >= @Start
                    AND Tab <= @End
                    ORDER BY LastMovedOn ASC",
                new
                {
                    UserId = userId,
                    Start = start,
                    End = end
                }, transaction);


            return div;

        }

        private async Task<Tab> GetMultipleCardTabOfLeastAndOldestCards(DbConnection dbConnection, DbTransaction transaction, long userId, Tab start, Tab end)
        {
            var tabCounts = await dbConnection.QueryAsync<(Tab Tab, int CardCount)>(
                @"SELECT Tab, COUNT(*) AS CardCount
                    FROM Cards
                    WHERE UserId = @UserId
                    AND Tab >= @Start
                    AND Tab <= @End
                    GROUP BY Tab
                    ORDER BY CardCount ASC, MIN(LastMovedOn) ASC",
                new
                {
                    UserId = userId,
                    Start = start,
                    End = end
                }, transaction);

            var emptyTabs = Enumerable.Range((int)start, (int)end - (int)start + 1).Select(i => (Tab)i)
                .Except(tabCounts
                    .Select(dc => dc.Tab))
                    .ToList();

            return emptyTabs.Any() ? emptyTabs.First() : tabCounts.First().Tab;

        }

        public async Task ReorderCard(long userId, long cardId, Tab tab, int newOrder)
        {
            var dbConnection = _db.GetDbConnection();

            if (dbConnection.State == System.Data.ConnectionState.Closed)
                dbConnection.Open();

            using (var transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    await ReorderCard_INTERNAL(dbConnection, transaction, userId, cardId, tab, newOrder);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private async Task ReorderCard_INTERNAL(DbConnection dbConnection, DbTransaction transaction, long userId, long cardId, Tab tab, int newOrder)
        {
            _logger.LogDebug("Reordering card {CardId} to order {NewOrder} in tab {Tab}", cardId, newOrder, tab);

            string sql = @"
                DECLARE @CurrentOrder INT;
                SELECT @CurrentOrder = [Order] FROM Cards WHERE Id = @CardId;

                -- Case 1: Moving up
                IF @NewOrder < @CurrentOrder
                BEGIN
                    UPDATE Cards
                    SET [Order] = [Order] + 1
                    WHERE [Order] >= @NewOrder AND [Order] < @CurrentOrder 
                        AND UserId = @UserId AND Tab = @Tab;

                    UPDATE Cards
                    SET [Order] = @NewOrder
                    WHERE Id = @CardId;
                END
                -- Case 2: Moving down
                ELSE IF @NewOrder > @CurrentOrder
                BEGIN
                    UPDATE Cards
                    SET [Order] = [Order] - 1
                    WHERE [Order] > @CurrentOrder AND [Order] <= @NewOrder 
                        AND UserId = @UserId AND Tab = @Tab;

                    UPDATE Cards
                    SET [Order] = @NewOrder
                    WHERE Id = @CardId;
                END

                -- Additional step to ensure unique order values
                -- Set the correct order for record with Id 327 if it’s still not ordered properly
                UPDATE Cards
                SET [Order] = 2
                WHERE Id = 327 AND UserId = @UserId AND Tab = @Tab AND [Order] = @NewOrder;";

                await dbConnection.ExecuteAsync(sql, new { UserId = userId, CardId = cardId, NewOrder = newOrder, Tab = tab }, transaction: transaction);
            }
        }

    }


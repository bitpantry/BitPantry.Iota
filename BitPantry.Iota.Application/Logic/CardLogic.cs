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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BitPantry.Iota.Application.Logic
{
    public class CardLogic
    {
        private ILogger<CardLogic> _logger;

        public CardLogic(ILogger<CardLogic> logger)
        {
            _logger = logger;
        }

        public async Task<bool> TryPromoteNextQueueCardCommand(DbConnection dbConnection, DbTransaction transaction, long userId)
        {
            // is daily open?

            var dailyCardCount = await dbConnection.QuerySingleAsync<int>(
                "SELECT COUNT(Id) FROM Cards WHERE UserId = @UserId and Tab = @DailyTab",
                new { userId, DailyTab = Tab.Daily}, transaction: transaction);

            if (dailyCardCount == 0)
            {
                // is there a card in the queue? If yes, move to daily and return true, otherwise return false

                var cardId = await dbConnection.QuerySingleOrDefaultAsync<long?>(
                    "SELECT TOP 1 Id FROM Cards WHERE UserId = @UserId AND Tab = @Tab ORDER BY [Order]",
                    new { userId, Tab = Tab.Queue }, transaction: transaction);

                if (cardId == null)
                    return false;

                await MoveCardCommand(dbConnection, transaction, cardId.Value, Tab.Daily);

                return true;
            }

            return false;
        }


        public async Task MoveCardCommand(DbConnection dbConnection, DbTransaction transaction, long cardId, Tab toTab)
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

            // Move the card and put in order 1 in the new tab

            string sql = @"
                DECLARE @CurrentOrder INT;
                DECLARE @CurrentTab INT;

                -- Get the current order and tab of the card being moved
                SELECT @CurrentOrder = [Order], @CurrentTab = Tab
                FROM Cards 
                WHERE Id = @CardId;

                -- Shift the orders in the new tab to make room for the moved card at order 1
                UPDATE Cards
                SET [Order] = [Order] + 1
                WHERE Tab = @ToTab 
                    AND UserId = @UserId 
                    AND [Order] >= 1;

                -- Move the card to the new tab and set its order to 1
                UPDATE Cards
                SET Tab = @ToTab, [Order] = 1, LastMovedOn = @Timestamp
                WHERE Id = @CardId;

                -- Adjust the order of cards in the old tab to fill any gaps
                UPDATE Cards
                SET [Order] = [Order] - 1
                WHERE Tab = @CurrentTab 
                    AND UserId = @UserId 
                    AND [Order] > @CurrentOrder;";

            // Parameters to pass to the query

            var parameters = new
            {
                CardId = cardId,
                ToTab = toTab,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            // Execute the query asynchronously

            await dbConnection.ExecuteAsync(sql, parameters, transaction);

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
                    var promotionTab = await GetPromotionTabQuery(dbConnection, transaction, userId, toTab);
                    _logger.LogDebug("Bumping card {CardId}", existingCardId);
                    await MoveCardCommand(dbConnection, transaction, existingCardId.Value, promotionTab);
                }
            }

            // if the card just moved was the daily card, try to promote the next queue card

            if (currentTab == (int)Tab.Daily)
                _ = await TryPromoteNextQueueCardCommand(dbConnection, transaction, userId);
        }

        public async Task<Tab> GetPromotionTabQuery(DbConnection dbConnection, DbTransaction transaction, long userId, Tab currentTab)
        {
            switch (currentTab)
            {
                case Tab.Queue:

                    return Tab.Daily;

                case Tab.Daily:

                    return await GetSingleCardTabOfOldestCardQuery(dbConnection, transaction, userId, Tab.Odd, Tab.Even);

                case Tab.Odd:
                case Tab.Even:

                    return await GetSingleCardTabOfOldestCardQuery(dbConnection, transaction, userId, Tab.Sunday, Tab.Saturday);

                case Tab.Sunday:
                case Tab.Monday:
                case Tab.Tuesday:
                case Tab.Wednesday:
                case Tab.Thursday:
                case Tab.Friday:
                case Tab.Saturday:

                    return await GetMultipleCardTabOfLeastAndOldestCardsQuery(dbConnection, transaction, userId, Tab.Day1, Tab.Day31);

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentTab), currentTab, "No promotion path is defined for this tab");
            }
        }

        private async Task<Tab> GetSingleCardTabOfOldestCardQuery(DbConnection dbConnection, DbTransaction transaction, long userId, Tab start, Tab end)
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

            var tab = await dbConnection.QuerySingleAsync<Tab>(
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


            return tab;

        }

        private async Task<Tab> GetMultipleCardTabOfLeastAndOldestCardsQuery(DbConnection dbConnection, DbTransaction transaction, long userId, Tab start, Tab end)
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

        public async Task ReorderCardCommand(DbConnection dbConnection, DbTransaction transaction, long userId, long cardId, Tab tab, int newOrder)
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
                END";

                await dbConnection.ExecuteAsync(sql, new { UserId = userId, CardId = cardId, NewOrder = newOrder, Tab = tab }, transaction: transaction);
            }
        }

    }


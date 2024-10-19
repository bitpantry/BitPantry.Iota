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
        private EntityDataContext _dbCtx;
        private ILogger<CardService> _logger;

        public CardService(EntityDataContext dbCtx, ILogger<CardService> logger)
        {
            _dbCtx = dbCtx;
            _logger = logger;
        }

        public async Task<GetCardResult> GetCard(long cardId)
        {
            // get card data, including verses

            var card = await GetCardQuery()
                .Where(c => c.Id == cardId)
                .FirstOrDefaultAsync();

            return BuildGetCardResult(card);

        }

        public async Task<GetCardResult> TryGetCard(long userId, Divider divider, int cardOrder)
        {
            // get card data, including verses

            var card = await GetCardQuery()
                .Where(c => c.Divider == divider && c.Order == cardOrder && c.UserId == userId)
                .FirstOrDefaultAsync();

            if(card == null)
                return null;

            return BuildGetCardResult(card);
        }

        private IQueryable<Card> GetCardQuery()
        {
            return _dbCtx.Cards
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
                card.Divider,
                card.Order,
                card.Verses);
        }

        public async Task PromoteDailyCard(long cardId)
        {
            _logger.LogDebug("Promoting daily card {CardId}", cardId);

            var dbConnection = _dbCtx.Database.GetDbConnection();

            // can this card be promoted - is it in the daily divider?

            var cardInfo = await _dbCtx.Database.GetDbConnection().QuerySingleOrDefaultAsync<dynamic>(
                "SELECT UserId, Divider FROM Cards WHERE Id = @CardId",
                new { cardId });

            if (cardInfo.Divider != Divider.Daily)
                throw new InvalidOperationException($"Only cards in the {Divider.Daily} divider can be promoted. This card is in the {(Divider)cardInfo.Divider} divider.");

            // begin a transaction

            var transaction = dbConnection.BeginTransaction();

            // try to promote from the queue to push everything up, or promote the daily card directly if no queue cards are available

            try
            {

                if (!await PromoteNextQueueCard(cardInfo.UserId, dbConnection, transaction))
                {
                    var promotionDivider = await GetPromotionDivider(dbConnection, transaction, cardInfo.UserId, cardInfo.Divider);
                    await MoveCard_RECURSIVE(dbConnection, transaction, cardId, promotionDivider);
                }

                transaction.Commit();

            }
            catch
            {
                transaction.Rollback();
                throw;
            }

        }

        private async Task<bool> PromoteNextQueueCard(long userId, DbConnection dbConnection, DbTransaction dbTransaction)
        {
            _logger.LogDebug("Promoting next card in the queue for user {UserId}", userId);

            // is there a card in the queue? If yes, move to daily and return true, otherwise return false

            var cardId = await dbConnection.QuerySingleOrDefaultAsync<long?>(
                "SELECT TOP 1 Id FROM Cards WHERE UserId = @UserId AND Divider = @Divider ORDER BY [Order]",
                new { userId, Divider.Queue });

            if (cardId == null)
                return false;

            await MoveCard_RECURSIVE(dbConnection, dbTransaction, cardId.Value, Divider.Daily);

            return true;
        }

        public async Task MoveCard(long cardId, Divider toDivider)
        {
            _logger.LogDebug("Moving card {CardId} to divider {ToDivider}", cardId, toDivider);

            var dbConnection = _dbCtx.Database.GetDbConnection();

            if (dbConnection.State == System.Data.ConnectionState.Closed)
                dbConnection.Open();

            using (var transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    await MoveCard_RECURSIVE(dbConnection, transaction, cardId, toDivider);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private async Task MoveCard_RECURSIVE(DbConnection dbConnection, DbTransaction transaction, long cardId, Divider toDivider)
        {
            // Get the current order and divider of the card to be moved
            var cardInfo = await dbConnection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT UserId, [Order], Divider FROM Cards WHERE Id = @CardId",
                new { cardId },
                transaction: transaction);

            if (cardInfo == null)
                throw new Exception("Card not found.");

            int currentOrder = cardInfo.Order;
            int currentDivider = cardInfo.Divider;
            long userId = cardInfo.UserId;

            // Update the divider of the card
            await dbConnection.ExecuteAsync(
                "UPDATE Cards SET Divider = @ToDivider, LastMovedOn = @Timestamp WHERE Id = @CardId",
            new { toDivider, cardId, Timestamp = DateTime.UtcNow },
                transaction: transaction);

            // Update the order of the remaining cards within the original divider
            await dbConnection.ExecuteAsync(
                "UPDATE Cards SET [Order] = [Order] - 1 WHERE Divider = @CurrentDivider AND UserId = @UserId AND [Order] > @CurrentOrder",
                new { CurrentDivider = currentDivider, UserId = userId, CurrentOrder = currentOrder },
                transaction: transaction);

            // Cascade promote cards in the destination divider if single card divider
            if(toDivider < Divider.Day1 && toDivider != Divider.Queue)
            {
                var existingCardId = await dbConnection.QuerySingleOrDefaultAsync<long?>(
                    @"SELECT Id
                      FROM Cards
                      WHERE UserId = @UserId
                      AND Divider = @ToDivider
                      AND Id != @CardId",
                    new { UserId = userId, ToDivider = toDivider, CardId = cardId },
                    transaction: transaction);

                if (existingCardId != null)
                {
                    var promotionDivider = await GetPromotionDivider(dbConnection, transaction, userId, toDivider);
                    _logger.LogDebug("Promoting card {CardId} :: {ToDivider} => {PromotionDivider}", existingCardId, toDivider, promotionDivider);
                    await MoveCard_RECURSIVE(dbConnection, transaction, existingCardId.Value, promotionDivider);
                }
            }
            else // put at the top of the list in any of the multi-card dividers
            {
                await ReorderCard_INTERNAL(dbConnection, transaction, userId, cardId, toDivider, 0);
            }
        }

        private async Task<Divider> GetPromotionDivider(DbConnection dbConnection, DbTransaction transaction, long userId, Divider currentDivider)
        {
            switch (currentDivider)
            {
                case Divider.Queue:

                    return Divider.Daily;

                case Divider.Daily:

                    return await GetSingleCardDividerOfOldestCard(dbConnection, transaction, userId, Divider.Odd, Divider.Even);

                case Divider.Odd:
                case Divider.Even:

                    return await GetMultipleCardDividerOfLeastAndOldestCards(dbConnection, transaction, userId, Divider.Sunday, Divider.Saturday);

                case Divider.Sunday:
                case Divider.Monday:
                case Divider.Tuesday:
                case Divider.Wednesday:
                case Divider.Thursday:
                case Divider.Friday:
                case Divider.Saturday:

                    return await GetSingleCardDividerOfOldestCard(dbConnection, transaction, userId, Divider.Day1, Divider.Day31);

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentDivider), currentDivider, "No promotion path is defined for this divider");
            }
        }

        private async Task<Divider> GetSingleCardDividerOfOldestCard(DbConnection dbConnection, DbTransaction transaction, long userId, Divider start, Divider end)
        {
            var div = await dbConnection.QuerySingleOrDefaultAsync<Divider?>(
                @"SELECT TOP 1 Divider
                    FROM Cards
                    WHERE UserId = @UserId
                    AND Divider >= @Start
                    AND Divider <= @End
                    ORDER BY LastMovedOn ASC",
                new
                {
                    UserId = userId,
                    Start = start,
                    End = end
                }, transaction);

            var emptyDividers = Enumerable.Range((int)start, (int)end - (int)start + 1).Select(i => (Divider)i)
                .Where(d => d != div)
                .ToList();

            return emptyDividers.Any() ? emptyDividers.First() : div.Value;
        }

        private async Task<Divider> GetMultipleCardDividerOfLeastAndOldestCards(DbConnection dbConnection, DbTransaction transaction, long userId, Divider start, Divider end)
        {
            var dividerCounts = await dbConnection.QueryAsync<(Divider Divider, int CardCount)>(
                @"SELECT Divider, COUNT(*) AS CardCount
                    FROM Cards
                    WHERE UserId = @UserId
                    AND Divider >= @Start
                    AND Divider <= @End
                    GROUP BY Divider
                    ORDER BY CardCount ASC, MIN(LastMovedOn) ASC",
                new
                {
                    UserId = userId,
                    Start = start,
                    End = end
                }, transaction);

            var emptyDividers = Enumerable.Range((int)start, (int)end - (int)start + 1).Select(i => (Divider)i)
                .Except(dividerCounts
                    .Select(dc => dc.Divider))
                    .ToList();

            return emptyDividers.Any() ? emptyDividers.First() : dividerCounts.First().Divider;

        }

        public async Task ReorderCard(long userId, long cardId, Divider divider, int newOrder)
        {
            var dbConnection = _dbCtx.Database.GetDbConnection();

            if (dbConnection.State == System.Data.ConnectionState.Closed)
                dbConnection.Open();

            using (var transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    await ReorderCard_INTERNAL(dbConnection, transaction, userId, cardId, divider, newOrder);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private async Task ReorderCard_INTERNAL(DbConnection dbConnection, DbTransaction transaction, long userId, long cardId, Divider divider, int newOrder)
        {
            _logger.LogDebug("Reordering card {CardId} to order {NewOrder} in divider {Divider}", cardId, newOrder, divider);

            string sql = @"
                DECLARE @CurrentOrder INT;
                SELECT @CurrentOrder = [Order] FROM Cards WHERE Id = @CardId;

                -- Case 1: Moving up
                IF @NewOrder < @CurrentOrder
                BEGIN
                    UPDATE Cards
                    SET [Order] = [Order] + 1
                    WHERE [Order] >= @NewOrder AND [Order] < @CurrentOrder AND UserId = @UserId AND Divider = @Divider;

                    UPDATE Cards
                    SET [Order] = @NewOrder
                    WHERE Id = @CardId;
                END

                -- Case 2: Moving down
                ELSE IF @NewOrder > @CurrentOrder
                BEGIN
                    UPDATE Cards
                    SET [Order] = [Order] - 1
                    WHERE [Order] > @CurrentOrder AND [Order] <= @NewOrder AND UserId = @UserId AND Divider = @Divider;

                    UPDATE Cards
                    SET [Order] = @NewOrder
                    WHERE Id = @CardId;
                END";

                await dbConnection.ExecuteAsync(sql, new { UserId = userId, CardId = cardId, NewOrder = newOrder, Divider = divider }, transaction: transaction);
            }
        }

    }


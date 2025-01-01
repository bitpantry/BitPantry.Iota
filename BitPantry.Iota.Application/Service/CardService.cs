using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Dapper;
using BitPantry.Iota.Application.DTO;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using System.ComponentModel;
using System.Threading;

namespace BitPantry.Iota.Application.Service
{
    public class CardService
    {
        private ILogger<CardService> _logger;
        private EntityDataContext _dbCtx;
        private CacheService _cacheSvc;
        private PassageLogic _passageLogic;

        public CardService(
            ILogger<CardService> logger,
            EntityDataContext dbCtx,
            CacheService cacheService,
            PassageLogic passageLogic) 
        {
            _logger = logger;
            _dbCtx = dbCtx;
            _cacheSvc = cacheService;
            _passageLogic = passageLogic;
        }

        public async Task<CreateCardResponse> CreateCard(long userId, long bibleId, string addressString, CancellationToken cancellationToken = default)
        {
            var tab = await _dbCtx.Cards.AnyAsync(c => c.UserId == userId && c.Tab == Tab.Daily)
                ? Tab.Queue
                : Tab.Daily;

            return await CreateCard(userId, bibleId, addressString, tab, cancellationToken);
        }

        public async Task<CreateCardResponse> CreateCard(long userId, long bibleId, string addressString, Tab toTab, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating new card :: {@Request}", new { userId, bibleId, addressString, toTab });

            // read the passage data

            var result = await _passageLogic.GetPassageQuery(_dbCtx, _cacheSvc, bibleId, addressString, cancellationToken);

            if(result.ParsingException != null)
            {
                var createRespCode = result.ParsingException.Code switch
                {
                    Parsers.PassageAddressParsingExceptionCode.InvalidAddress => CreateCardResponseResult.InvalidAddress,
                    Parsers.PassageAddressParsingExceptionCode.BookNameUnresolved => CreateCardResponseResult.BookNameUnresolved,
                    _ => throw new ArgumentOutOfRangeException(nameof(result.ParsingException.Code), "This value is not defined for this switch - cannot be mapped to a CreateCardResponseCode"),
                };

                return new CreateCardResponse(createRespCode, null);
            }

            // see if the user already has this card created

            if (await _dbCtx.Cards.DoesCardAlreadyExistForPassage(userId, result.Passage.GetAddressString(), cancellationToken))
                return new CreateCardResponse(CreateCardResponseResult.CardAlreadyExists, null);

            // create the card

            var card = result.Passage.ToCard(
                userId,
                toTab,
                await _dbCtx.Cards.GetNewLastFractionalOrder(userId, toTab, cancellationToken));

            _dbCtx.Cards.Add(card);
            await _dbCtx.SaveChangesAsync(cancellationToken);

            card = await _dbCtx.Cards.Where(c => c.Id == card.Id).SingleAsync();

            _dbCtx.ChangeTracker.Clear();

            // return the response

            return new CreateCardResponse(CreateCardResponseResult.Ok, card.ToDto());
        }

        public async Task<bool> DoesCardAlreadyExistForUser(long userId, long bibleId, string addressString, CancellationToken cancellationToken)
        {
            var result = await _passageLogic.GetPassageQuery(_dbCtx, _cacheSvc, bibleId, addressString, cancellationToken);
            return await _dbCtx.Cards.DoesCardAlreadyExistForPassage(userId, result.Passage.GetAddressString(), cancellationToken);
        }

        internal async Task DeleteCard(long cardId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Deleting card {CardId}", cardId);

            // get card tab

            var tabInt = await _dbCtx.UseConnection(cancellationToken, async conn => await conn.QuerySingleOrDefaultAsync<int?>("SELECT Tab FROM Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE Id = @Id", new { Id = cardId }));

            if (!tabInt.HasValue)
                throw new InvalidOperationException($"The card, {cardId}, does not exist");

            // delete the card

            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                await conn.ExecuteAsync(@"DELETE FROM Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE Id = @CardId;",
                    new { cardId },
                    transaction: trans);
            });
        }

        public async Task DeleteAllCards(long? forUserId, CancellationToken cancellationToken)
        {
            if (forUserId.HasValue)
            {
                _logger.LogDebug("Deleting all cards for user {ForUserId}", forUserId.Value);
                await _dbCtx.UseConnection(cancellationToken, async (conn, trans) => await conn.ExecuteAsync("DELETE FROM Cards WHERE UserId = @UserId", new { UserId = forUserId }, transaction: trans));
            }
            else
            {
                _logger.LogWarning("Deleting all cards for all users!");
                await _dbCtx.UseConnection(cancellationToken, async (conn, trans) => await conn.ExecuteAsync("DELETE FROM Cards", transaction: trans));
            }
        }

        public async Task<CardDto> GetCard(long cardId, CancellationToken cancellationToken = default)
        {
            //var cards = await _dbCtx.UseConnection(cancellationToken, async (conn) =>
            //{
            //    return await conn.QueryAsync<Card>(@"
            //        WITH RankedCards AS (
            //            SELECT 
            //                c.*,
            //                ROW_NUMBER() OVER (PARTITION BY c.UserId, c.Tab ORDER BY c.FractionalOrder) AS RowNumber
            //            FROM Cards c
            //            WHERE c.UserId = (SELECT UserId FROM Cards WHERE Id = @CardId) AND c.Tab = (SELECT Tab FROM Cards WHERE Id = @CardId)
            //        )
            //        SELECT *
            //        FROM RankedCards
            //        WHERE Id = @CardId;",
            //        new { CardId = cardId });
            //});

            var card = await _dbCtx.Cards.Where(c => c.Id == cardId).SingleOrDefaultAsync(cancellationToken);

            return await GetCard_INTERNAL(card, true, cancellationToken);
        }

        public async Task<CardDto> GetCard(long userId, Tab tab, int rowNumber, CancellationToken cancellationToken = default)
        {
            //var cards = await _dbCtx.UseConnection(cancellationToken, async (conn) =>
            //{
            //    return await conn.QueryAsync<Card>(@"
            //        WITH RankedCards AS (
            //            SELECT 
            //                c.*,
            //                ROW_NUMBER() OVER (PARTITION BY c.UserId, c.Tab ORDER BY c.FractionalOrder) AS RowNumber
            //            FROM Cards c
            //            WHERE c.UserId = @UserId AND c.Tab = @Tab
            //        )
            //        SELECT *
            //        FROM RankedCards
            //        WHERE RowNumber = @RowNumber;",
            //        new { UserId = userId, Tab = (int)tab, RowNumber = rowNumber });
            //});

            var card = await _dbCtx.Cards.Where(c => c.UserId == userId && c.Tab == tab && c.NumberedCard.RowNumber == rowNumber).SingleOrDefaultAsync(cancellationToken);

            return await GetCard_INTERNAL(card, true, cancellationToken);
        }

        private async Task<CardDto> GetCard_INTERNAL(Card card, bool includePassage, CancellationToken cancellationToken)
        {
            if (card == null)
                return null;

            // get verses

            var verses = includePassage ? await _dbCtx.Verses.ToListAsync(card.StartVerseId, card.EndVerseId, cancellationToken) : null;

            _dbCtx.ChangeTracker.Clear();

            // return card dto

            return card.ToDto(verses);
        }

        public async Task<int> GetCardCountForUser(long userId, CancellationToken cancellationToken)
            => await _dbCtx.Cards.CountAsync(c => c.UserId == userId, cancellationToken);

        internal async Task MoveCard(long cardId, Tab toTab, bool toTop = true, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Moving card {CardId} to tab {ToTab} (to top: {ToTop}", cardId, toTab, toTop);           

            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                var sql = @"
                    DECLARE @MaxFractionalOrder FLOAT;
                    DECLARE @MinFractionalOrder FLOAT;
                    DECLARE @UserId BIGINT;

                    -- Get the UserId for the given CardId
                    SELECT @UserId = UserId
                    FROM Cards
                    WHERE Id = @CardId;

                    -- Get the max and min fractional orders for the given tab and user
                    SELECT 
                        @MaxFractionalOrder = MAX(FractionalOrder), 
                        @MinFractionalOrder = MIN(FractionalOrder)
                    FROM Cards
                    WHERE Tab = @ToTab AND UserId = @UserId;

                    -- Handle scenarios where there are no cards in the target tab
                    IF @MaxFractionalOrder IS NULL OR @MinFractionalOrder IS NULL
                    BEGIN
                        -- Default values when there are no cards
                        SET @MaxFractionalOrder = 0; -- will resolve to 1
                        SET @MinFractionalOrder = 2; -- will resolve to 1
                    END

                    -- Update the card's tab and fractional order
                    UPDATE Cards
                    SET 
                        Tab = @ToTab,
                        ReviewCount = 0,
                        FractionalOrder = CASE 
                                            WHEN @ToTop = 1 THEN @MinFractionalOrder - 1 -- Place above the first card
                                            ELSE @MaxFractionalOrder + 1 -- Place below the last card
                                          END
                    WHERE Id = @CardId;";

                var args = new
                {
                    CardId = cardId,
                    ToTab = toTab,
                    ToTop = toTop,
                    Timestamp = DateTime.UtcNow
                };

                await conn.ExecuteAsync(sql, args, trans);
            });
        }

        public async Task ReorderCard(long userId, Tab tab, long cardId, long newRowNumber, CancellationToken cancellationToken)
        {
            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                _logger.LogDebug("Reordering card {CardId} to order {NewRowNumber} in tab {Tab}", cardId, newRowNumber, tab);

                string sql = @"
                    DECLARE @PrevFractionalOrder FLOAT;
                    DECLARE @NextFractionalOrder FLOAT;
                    DECLARE @NewFractionalOrder FLOAT;

                    -- Get the FractionalOrder of the card at RowNumber = newRowNumber - 1 (previous card)
                    SELECT @PrevFractionalOrder = FractionalOrder
                    FROM (
                        SELECT 
                            FractionalOrder, 
                            ROW_NUMBER() OVER (PARTITION BY UserId, Tab ORDER BY FractionalOrder) AS RowNumber
                        FROM Cards
                        WHERE Tab = @Tab AND UserId = @UserId AND ID <> @CardId
                    ) AS OrderedCards
                    WHERE RowNumber < @NewRowNumber;

                    -- Get the FractionalOrder of the card at RowNumber = newRowNumber (next card)
                    SELECT @NextFractionalOrder = FractionalOrder
                    FROM (
                        SELECT 
                            FractionalOrder, 
                            ROW_NUMBER() OVER (PARTITION BY UserId, Tab ORDER BY FractionalOrder) AS RowNumber
                        FROM Cards
                        WHERE Tab = @Tab AND UserId = @UserId AND Id <> @CardId
                    ) AS OrderedCards
                    WHERE RowNumber >= @NewRowNumber
					ORDER BY RowNumber DESC

                    -- Handle cases when both previous and next fractional orders are NULL
                    IF @PrevFractionalOrder IS NULL AND @NextFractionalOrder IS NULL
                    BEGIN
                        SET @NewFractionalOrder = 1; -- Default value when the tab is empty
                    END
                    ELSE IF @PrevFractionalOrder IS NULL
                    BEGIN
                        SET @NewFractionalOrder = @NextFractionalOrder - 1; -- Place at the top
                    END
                    ELSE IF @NextFractionalOrder IS NULL
                    BEGIN
                        SET @NewFractionalOrder = @PrevFractionalOrder + 1; -- Place at the bottom
                    END
                    ELSE
                    BEGIN
                        SET @NewFractionalOrder = (@PrevFractionalOrder + @NextFractionalOrder) / 2; -- Place in between
                    END;

                    -- Update the card's FractionalOrder
                    UPDATE Cards
                    SET FractionalOrder = @NewFractionalOrder
                    WHERE Id = @CardId;";

                await conn.ExecuteAsync(sql, new { UserId = userId, CardId = cardId, NewRowNumber = newRowNumber, Tab = tab }, transaction: trans);
            });
        }

        public async Task MarkAsReviewed(long cardId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Marking card {CardId} as reviewed", cardId);

            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                await conn.ExecuteAsync("UPDATE Cards SET LastReviewedOn = @Timestamp, ReviewCount = ReviewCount + 1 WHERE Id = @CardId",
                    new
                    {
                        Timestamp = DateTime.UtcNow,
                        Cardid = cardId
                    }, trans);
            });
        }

        public async Task MarkAsReviewed(long userId, Tab tab, long rowNumber, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Marking card {Tab}:{RowNumber} as reviewed", tab, rowNumber);

            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                await conn.ExecuteAsync(@"
                    DECLARE @CardId BIGINT;

                    -- Find the card ID based on the given RowNumber
                    SELECT @CardId = Id
                    FROM (
                        SELECT 
                            ROW_NUMBER() OVER (PARTITION BY UserId, Tab ORDER BY FractionalOrder) AS RowNumber,
                            Id
                        FROM Cards
                        WHERE Tab = @Tab AND UserId = @UserId
                    ) AS OrderedCards
                    WHERE RowNumber = @RowNumber;

                    -- Increment the ReviewCount for the found card
                    UPDATE Cards
                    SET LastReviewedOn = @Timestamp, ReviewCount = ReviewCount + 1
                    WHERE Id = @CardId;",
                    new
                    {
                        Timestamp = DateTime.UtcNow,
                        UserId = userId,
                        Tab = tab,
                        RowNumber = rowNumber
                    }, trans);
            });
        }

        public async Task<CardDto> GetNextQueueCard(long userId, CancellationToken cancellationToken = default)
        {
            var card = await _dbCtx.Cards.Where(c => c.UserId == userId && c.Tab == Tab.Queue).OrderBy(c => c.NumberedCard.RowNumber).FirstOrDefaultAsync(cancellationToken);
            return await GetCard_INTERNAL(card, true, cancellationToken);
        }

        public async Task ResetReviewCount(long id, CancellationToken cancellationToken = default)
        {
            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                await conn.ExecuteAsync("UPDATE Cards SET ReviewCount = 0 WHERE Id = @Id", new { Id = id }, trans);
            });
        }
    }
}

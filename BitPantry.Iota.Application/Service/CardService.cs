using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Dapper;
using BitPantry.Iota.Application.DTO;
using System.Data.Common;
using Polly;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using System.ComponentModel;

namespace BitPantry.Iota.Application.Service
{
    public class CardService
    {
        private ILogger<CardService> _logger;
        private EntityDataContext _dbCtx;
        private CacheService _cacheSvc;
        private PassageLogic _passageLogic;

        //private readonly ResiliencePipeline _deadlockResiliencyPipeline = Policy
        //    .Handle<SqlException>(ex => ex.Message.Contains("deadlocked on lock resources"))
        //    .Or<TimeoutException>()
        //    .WaitAndRetryAsync()

        private readonly ResiliencePipeline _sqlResiliencyPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<SqlException>(ex => ex.Message.Contains("deadlocked on lock resources")),
                BackoffType = DelayBackoffType.Linear,
                UseJitter = true,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(50)
            })
            .Build();

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
            => await CreateCard(userId, bibleId, addressString, toTab, null, cancellationToken);

        public async Task<CreateCardResponse> CreateCard(long userId, long bibleId, string addressString, Tab toTab, int? order, CancellationToken cancellationToken)
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

            // check if card order is already used

            if(order.HasValue)
            {
                if (await _dbCtx.Cards.Where(c => c.UserId == userId && c.Tab == toTab && c.Order == order.Value).Select(c => c.Id).AnyAsync(cancellationToken))
                    throw new InvalidOperationException($"Order {order.Value} is already taken");
            }

            // create the card

            var card = result.Passage.ToCard(
                userId,
                toTab,
                order.HasValue ? order.Value : await _dbCtx.Cards.GetNextAvailableOrder(userId, toTab, cancellationToken));

            _dbCtx.Cards.Add(card);
            await _dbCtx.SaveChangesAsync(cancellationToken);

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

            var tabInt = await _dbCtx.UseConnection(cancellationToken, async conn => await conn.QuerySingleOrDefaultAsync<int?>("SELECT Tab FROM Cards WHERE Id = @Id", new { Id = cardId }));

            if (!tabInt.HasValue)
                throw new InvalidOperationException($"The card, {cardId}, does not exist");

            // delete the card

            //await _sqlResiliencyPipeline.ExecuteAsync(async token =>
            //{
                await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
                {
                    //await conn.ExecuteAsync(@"
                    //DECLARE @CurrentOrder INT;
                    //DECLARE @CurrentTab INT;
                    //DECLARE @UserId BIGINT;

                    //SELECT @CurrentOrder = [Order], @CurrentTab = Tab, @UserId = UserId
                    //FROM Cards 
                    //WHERE Id = @CardId;

                    //DELETE FROM Cards WHERE Id = @CardId;

                    //UPDATE Cards SET [Order] = [Order] - 1 WHERE Tab = @CurrentTab AND UserId = @UserId AND [Order] > @CurrentOrder;",
                    //    new { cardId },
                    //    transaction: trans);

                    await conn.ExecuteAsync(@"
                    DECLARE @CurrentOrder INT;
                    DECLARE @CurrentTab INT;
                    DECLARE @UserId BIGINT;

                    SELECT @CurrentOrder = [Order], @CurrentTab = Tab, @UserId = UserId
                    FROM Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
                    WHERE Id = @CardId;

                    UPDATE Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
                    SET [Order] = [Order] - 1
                    WHERE Tab = @CurrentTab AND UserId = @UserId AND [Order] > @CurrentOrder;

                    DELETE FROM Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
                    WHERE Id = @CardId;
                    ",
                        new { cardId },
                        transaction: trans);
                });
            //}, cancellationToken);
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
            return await GetCard_INTERNAL(_dbCtx.Cards.AsNoTracking().Where(c => c.Id == cardId), true, cancellationToken);
        }

        public async Task<CardDto> GetCard(long userId, Tab tab, int order, CancellationToken cancellationToken = default)
        {
            return await GetCard_INTERNAL(_dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == tab && c.Order == order), true, cancellationToken);
        }

        private async Task<CardDto> GetCard_INTERNAL(IQueryable<Card> cardQuery, bool includePassage, CancellationToken cancellationToken)
        {
            var card = await cardQuery.SingleOrDefaultAsync(cancellationToken);

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

            // Determine the order placement in the new tab

            //await _sqlResiliencyPipeline.ExecuteAsync(async token =>
            //{
                await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
                {
                    var sql = @"
                        DECLARE @CurrentOrder INT;
                        DECLARE @CurrentTab   INT;
                        DECLARE @NewOrder     INT;
                        DECLARE @UserId       BIGINT;

                        -- 1) Get the card’s current order/tab
                        SELECT @CurrentOrder = [Order],
                               @CurrentTab   = [Tab],
                               @UserId       = [UserId]
                        FROM Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
                        WHERE Id = @CardId;

                        -- 2) Decrement orders in old tab
                        UPDATE Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
                        SET [Order] = [Order] - 1
                        WHERE [Tab]   = @CurrentTab
                          AND UserId  = @UserId
                          AND [Order] > @CurrentOrder;

                        -- 3) If @ToTop = 1, move to top by shifting everyone else up
                        IF (@ToTop = 1)
                        BEGIN
                            UPDATE Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
                            SET [Order] = [Order] + 1
                            WHERE [Tab]  = @ToTab
                              AND UserId = @UserId
                              AND [Order] >= 1;

                            SET @NewOrder = 1;
                        END
                        ELSE
                        BEGIN
                            SELECT @NewOrder = ISNULL(MAX([Order]), 0) + 1
                            FROM Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
                            WHERE [Tab]  = @ToTab
                              AND UserId = @UserId;
                        END

                        -- 4) Move the card
                        UPDATE Cards WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
                        SET [Tab]        = @ToTab,
                            [Order]      = @NewOrder,
                            LastMovedOn  = @Timestamp,
                            ReviewCount  = 0
                        WHERE Id = @CardId
                          AND UserId = @UserId;
                    ";

                    var args = new
                    {
                        CardId = cardId,
                        ToTab = toTab,
                        ToTop = toTop,
                        Timestamp = DateTime.UtcNow
                    };

                    await conn.ExecuteAsync(sql, args, trans);
                });
            //}, cancellationToken);

        }

        public async Task ReorderCard(long userId, Tab tab, long cardId, int newOrder, CancellationToken cancellationToken)
        {
            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
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

                await conn.ExecuteAsync(sql, new { UserId = userId, CardId = cardId, NewOrder = newOrder, Tab = tab }, transaction: trans);
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

        public async Task MarkAsReviewed(long userId, Tab tab, int cardOrder, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Marking card {Tab}:{CardOrder} as reviewed", tab, cardOrder);

            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                await conn.ExecuteAsync("UPDATE Cards SET LastReviewedOn = @Timestamp, ReviewCount = ReviewCount + 1 WHERE UserId = @UserId AND Tab = @Tab AND [Order] = @Order",
                    new
                    {
                        Timestamp = DateTime.UtcNow,
                        UserId = userId,
                        Tab = tab,
                        Order = cardOrder
                    }, trans);
            });
        }
    }

    
}

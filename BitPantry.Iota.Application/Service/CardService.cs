using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Dapper;
using BitPantry.Iota.Application.DTO;

namespace BitPantry.Iota.Application.Service
{
    public class CardService
    {
        private ILogger<CardService> _logger;
        private EntityDataContext _dbCtx;
        private CacheService _cacheSvc;
        private PassageLogic _passageLogic;
        private CardLogic _cardLogic;

        public CardService(
            ILogger<CardService> logger,
            EntityDataContext dbCtx,
            CacheService cacheService,
            PassageLogic passageLogic,
            CardLogic cardLogic) 
        {
            _logger = logger;
            _dbCtx = dbCtx;
            _cacheSvc = cacheService;
            _passageLogic = passageLogic;
            _cardLogic = cardLogic;
        }

        public async Task<CreateCardResponse> CreateCard(long userId, long bibleId, string addressString, CancellationToken cancellationToken)
        {
            var tab = await _dbCtx.Cards.AnyAsync(c => c.UserId == userId && c.Tab == Tab.Daily)
                ? Tab.Queue
                : Tab.Daily;

            return await CreateCard(userId, bibleId, addressString, tab, cancellationToken);
        }

        public async Task<CreateCardResponse> CreateCard(long userId, long bibleId, string addressString, Tab toTab, CancellationToken cancellationToken)
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

            // return the response

            return new CreateCardResponse(CreateCardResponseResult.Ok, card.ToDto());
        }

        public async Task<bool> DoesCardAlreadyExistForUser(long userId, long bibleId, string addressString, CancellationToken cancellationToken)
        {
            var result = await _passageLogic.GetPassageQuery(_dbCtx, _cacheSvc, bibleId, addressString, cancellationToken);
            return await _dbCtx.Cards.DoesCardAlreadyExistForPassage(userId, result.Passage.GetAddressString(), cancellationToken);
        }

        public async Task DeleteAllCards(long? forUserId, CancellationToken cancellationToken)
        {
            if (forUserId.HasValue)
            {
                _logger.LogDebug("Deleting aall cards for user {ForUserId}", forUserId.Value);
                await _dbCtx.UseConnection(cancellationToken, async (conn, trans) => await conn.ExecuteAsync("DELETE FROM Cards WHERE UserId = @UserId", new { UserId = forUserId }, transaction: trans));
            }
            else
            {
                _logger.LogWarning("Deleting all cards for all users!");
                await _dbCtx.UseConnection(cancellationToken, async (conn, trans) => await conn.ExecuteAsync("DELETE FROM Cards", transaction: trans));
            }               
        }

        public async Task DeleteCard(long cardId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Deleting card {CardId}", cardId);

            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                // Get the current order and tab of the card to be deleted

                var cardInfo = conn.QuerySingleOrDefault<dynamic>(
                    "SELECT [Order], UserId, Tab FROM Cards WHERE Id = @CardId",
                    new { cardId },
                    transaction: trans);

                if (cardInfo == null)
                    throw new Exception("Card not found.");

                int currentOrder = cardInfo.Order;
                int tab = cardInfo.Tab;
                long userId = cardInfo.UserId;

                // Delete the card
                await conn.ExecuteAsync(
                    "DELETE FROM Cards WHERE Id = @CardId",
                    new { cardId },
                    transaction: trans);

                // Update the order of the remaining cards within the same tab
                await conn.ExecuteAsync(
                    "UPDATE Cards SET [Order] = [Order] - 1 WHERE Tab = @Tab AND UserId = @UserId AND [Order] > @CurrentOrder",
                    new { Tab = tab, UserId = userId, CurrentOrder = currentOrder },
                    transaction: trans);

                // if the card was in the daily tab, promote the next queued card

                if (tab == (int)Tab.Daily)
                    _ = await _cardLogic.TryPromoteNextQueueCardCommand(conn, trans, userId);
            });
        }

        public async Task MarkCardAsReviewed(long userId, Tab tab, int cardOrder, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Marking card {Tab}:{CardOrder} as reviewed", tab, cardOrder);

            var card = await _dbCtx.Cards
                .Where(c => c.UserId == userId)
                .Where(c => c.Tab == tab)
                .Where(c => c.Order == cardOrder)
                .SingleAsync(cancellationToken);

            card.LastReviewedOn = DateTime.UtcNow;

            await _dbCtx.SaveChangesAsync(cancellationToken);
        }

        public async Task MoveCard(long cardId, Tab toTab, bool atTop, CancellationToken cancellationToken)
        {
            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) => await _cardLogic.MoveCardCommand(conn, trans, cardId, toTab, atTop));
        }

        public async Task PromoteDailyCard(long cardId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Promoting daily card {CardId}", cardId);

            // can this card be promoted - is it in the daily tab?

            var cardInfo = await _dbCtx.UseConnection(cancellationToken, async (conn) =>
            {
                return await conn.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT UserId, Tab FROM Cards WHERE Id = @CardId",
                new { cardId });
            });

            long userId = cardInfo.UserId;
            Tab tab = (Tab)cardInfo.Tab;

            if (tab != Tab.Daily)
                throw new InvalidOperationException($"Only cards in the {Tab.Daily} tab can be promoted. This card is in the {(Tab)tab} tab.");

            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                // set the current card's last reviewed timestamp

                await conn.ExecuteAsync(
                    "UPDATE Cards SET LastReviewedOn = @Timestamp WHERE Id = @CardId",
                    new { Timestamp = DateTime.UtcNow, CardId = cardId },
                    transaction: trans);

                // try to promote the queue card if one exists

                if (!await _cardLogic.TryPromoteNextQueueCardCommand(conn, trans, userId))
                {
                    var promotionTab = await _cardLogic.GetPromotionTabQuery(conn, trans, userId, tab);
                    await _cardLogic.MoveCardCommand(conn, trans, cardId, promotionTab);
                }
            });
        }

        public async Task ReorderCard(long userId, Tab tab, long cardId, int newOrder, CancellationToken cancellationToken)
        {
            await _dbCtx.UseConnection(cancellationToken, async (conn, trans)
                => await _cardLogic.ReorderCardCommand(conn, trans, userId, cardId, tab, newOrder));
        }

        public async Task<CardDto> GetCard(long cardId, CancellationToken cancellationToken)
        {
            return await GetCard_INTERNAL(_dbCtx.Cards.AsNoTracking().Where(c => c.Id == cardId), true, cancellationToken);
        }

        public async Task<CardDto> GetCard(long userId, Tab tab, int order, CancellationToken cancellationToken)
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

            // return card dto

            return card.ToDto(verses);
        }

        public async Task<int> GetUserCardCount(long userId, CancellationToken cancellationToken)
            => await _dbCtx.Cards.CountAsync(c => c.UserId == userId, cancellationToken);

        public async Task SwapDailyWithQueue(long userId, long queueCardId, CancellationToken cancellationToken)
        {
            // get the queue card

            var queueCard = await _dbCtx.Cards.FirstOrDefaultAsync(c => c.Id == queueCardId, cancellationToken);

            if (queueCard.Tab != Tab.Queue)
                throw new InvalidOperationException($"Card with id {queueCardId} is expected to be in the queue");

            // get the daily card and make updates

            var dailyCard = await _dbCtx.Cards.FirstOrDefaultAsync(c => c.UserId == userId && c.Tab == Tab.Daily);

            await _dbCtx.UseConnection(cancellationToken, async (conn, trans) =>
            {
                if(dailyCard != null)
                    await _cardLogic.MoveCardCommand(conn, trans, dailyCard.Id, Tab.Queue, true, false);
                await _cardLogic.MoveCardCommand(conn, trans, queueCardId, Tab.Daily, true, false);
            });

            await _dbCtx.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetCardCountForTab(long userId, Tab tab, CancellationToken cancellationToken)
            => await _dbCtx.Cards.CountAsync(c => c.UserId == userId && c.Tab == tab, cancellationToken);
        
    }

    
}

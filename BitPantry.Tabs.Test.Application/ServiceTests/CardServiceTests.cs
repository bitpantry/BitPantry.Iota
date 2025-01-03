using BitPantry.Tabs.Application;
using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Test.Application.Fixtures;
using Dapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BitPantry.Tabs.Test.Application.ServiceTests
{
    [Collection("env")]
    public class CardServiceTests 
    {
        private static long _bibleId;
        private readonly ApplicationEnvironment _env;
        
        public CardServiceTests(AppEnvironmentFixture fixture)
        {
            _bibleId = fixture.BibleId;
            _env = fixture.Environment;
        }

        [Theory]
        [InlineData("rom 1:16")]
        [InlineData("1 tim 3:16")]
        [InlineData("gen 12:1-14")]
        [InlineData("1 tim 3:4-4:1")]
        public async Task CreateCard_CardCreated(string address)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await svc.CreateCard(userId, _bibleId, address, CancellationToken.None);

                resp.Result.Should().Be(CreateCardResponseResult.Ok);
                
                resp.Card.Should().NotBeNull();
                resp.Card.Address.Should().NotBeNullOrEmpty();
                resp.Card.AddedOn.Date.Should().Be(DateTime.UtcNow.Date);
                resp.Card.LastMovedOn.Date.Should().Be(DateTime.UtcNow.Date);
                resp.Card.LastReviewedOn.Should().BeNull();
                resp.Card.RowNumber.Should().BeGreaterThan(0);
            }
        }

        [Theory]
        [InlineData("x")]
        [InlineData("hez 3")]
        [InlineData("1 tim 3:4-2 tim 1:1")]
        public async Task CreateCardInvalidAddress_InvalidAddressResult(string address)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await svc.CreateCard(userId, _bibleId, address, CancellationToken.None);

                resp.Result.Should().Be(CreateCardResponseResult.InvalidAddress);
                resp.Card.Should().BeNull();
            }
        }

        [Fact]
        public async Task CreateTwoCards_CreatedInQueueAndDaily()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp1 = await svc.CreateCard(userId, _bibleId, "gen 1:1", CancellationToken.None);
                var resp2 = await svc.CreateCard(userId, _bibleId, "gen 1:2", CancellationToken.None);

                resp1.Card.Tab.Should().Be(Common.Tab.Daily);
                resp2.Card.Tab.Should().Be(Common.Tab.Queue);
            }
        }

        [Fact]
        public async Task CreateCardEmptyAddress_ArgumentNullException()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var act = async () => { await svc.CreateCard(userId, _bibleId, "", CancellationToken.None); };

                await act.Should().ThrowAsync<ArgumentNullException>();
            }
        }

        [Fact]
        public async Task CreateMultipleCards_CardsCreatedWithCorrectOrder()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp1 = await svc.CreateCard(userId, _bibleId, "gen 12:1", Common.Tab.Queue, CancellationToken.None);
                var resp2 = await svc.CreateCard(userId, _bibleId, "gen 12:2", Common.Tab.Queue, CancellationToken.None);
                var resp3 = await svc.CreateCard(userId, _bibleId, "gen 12:3", Common.Tab.Queue, CancellationToken.None);

                resp1.Card.RowNumber.Should().Be(1);
                resp2.Card.RowNumber.Should().Be(2);
                resp3.Card.RowNumber.Should().Be(3);
            }
        }

        [Theory]
        [InlineData("x 4:1")]
        [InlineData("hez 3:1")]
        [InlineData("j 3:7")]
        public async Task CreateCardBadBookName_BookNameUnresolvedResult(string address)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await svc.CreateCard(userId, _bibleId, address, CancellationToken.None);

                resp.Result.Should().Be(CreateCardResponseResult.BookNameUnresolved);
                resp.Card.Should().BeNull();
            }
        }

        [Fact]
        public async Task CreateDuplicateCard_CardAlreadyExistsResult()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await svc.CreateCard(userId, _bibleId, "jn 3:16", CancellationToken.None);

                resp = await svc.CreateCard(userId, _bibleId, "jn 3:16", CancellationToken.None);
                resp.Result.Should().Be(CreateCardResponseResult.CardAlreadyExists);
            }
        }

        [Fact]
        public async Task DeleteAllCards_AllCardsDeleted()
        {
            var user1Id = await _env.CreateUser();
            var user2Id = await _env.CreateUser();

            _ = await _env.CreateCards(user1Id, _bibleId);
            _ = await _env.CreateCards(user2Id, _bibleId);

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var count = await dbCtx.UseConnection(CancellationToken.None, async conn => 
                {
                    return await conn.QuerySingleAsync<long>("SELECT COUNT(*) FROM Cards");
                });

                count.Should().BeGreaterThan(0);

                await svc.DeleteAllCards(null, CancellationToken.None);

                count = await dbCtx.UseConnection(CancellationToken.None, async conn =>
                {
                    return await conn.QuerySingleAsync<long>("SELECT COUNT(*) FROM Cards");
                });

                count.Should().Be(0);

            }
        }

        [Fact]
        public async Task DeleteAllCardsForUser_AllCardsDeletedForUser()
        {
            var user1Id = await _env.CreateUser();
            var user2Id = await _env.CreateUser();

            _ = await _env.CreateCards(user1Id, _bibleId);
            _ = await _env.CreateCards(user2Id, _bibleId);

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                await svc.DeleteAllCards(user1Id, CancellationToken.None);

                var count = await dbCtx.UseConnection(CancellationToken.None, async conn =>
                {
                    return await conn.QuerySingleAsync<long>("SELECT COUNT(*) FROM Cards WHERE UserId = @UserId", new { UserId = user1Id });
                });

                count.Should().Be(0);

                count = await dbCtx.UseConnection(CancellationToken.None, async conn =>
                {
                    return await conn.QuerySingleAsync<long>("SELECT COUNT(*) FROM Cards WHERE UserId = @UserId", new { UserId = user2Id });
                });

                count.Should().NotBe(0);
            }
        }

        [Fact]
        public async Task MarkCardAsReviewed_CardMarked()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var resp = await svc.CreateCard(userId, _bibleId, "rom 1:16", CancellationToken.None);

                resp.Card.LastReviewedOn.Should().BeNull();
                resp.Card.ReviewCount.Should().Be(0);
                resp.Card.LastReviewedOn.Should().Be(null);

                await svc.MarkAsReviewed(userId, resp.Card.Tab, resp.Card.RowNumber, CancellationToken.None);

                var card = await svc.GetCard(resp.Card.Id, CancellationToken.None);

                card.LastReviewedOn.Value.Date.Should().Be(DateTime.UtcNow.Date);
                card.ReviewCount.Should().Be(1);
                card.LastReviewedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ResetReviewCount_ReviewCountReset()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var resp = await svc.CreateCard(userId, _bibleId, "rom 1:16", CancellationToken.None);

                resp.Card.LastReviewedOn.Should().BeNull();
                resp.Card.ReviewCount.Should().Be(0);
                resp.Card.LastReviewedOn.Should().Be(null);

                await svc.MarkAsReviewed(userId, resp.Card.Tab, resp.Card.RowNumber, CancellationToken.None);

                var card = await svc.GetCard(resp.Card.Id, CancellationToken.None);

                card.LastReviewedOn.Value.Date.Should().Be(DateTime.UtcNow.Date);
                card.ReviewCount.Should().Be(1);
                card.LastReviewedOn.Should().NotBeNull();

                await svc.ResetReviewCount(resp.Card.Id);

                card = await svc.GetCard(resp.Card.Id, CancellationToken.None);

                card.ReviewCount.Should().Be(0);
            }
        }

        [Theory]
        [InlineData(1L, 2L)]
        [InlineData(null, 1L)]
        [InlineData(1L, null)]
        [InlineData(2L, 1L)]
        [InlineData(3L, 6L)]
        [InlineData(6L, 1L)]
        [InlineData(null, null)]
        [InlineData(1L, 1L)]
        public async Task ReorderCard_CardReorderedAndTabOrderUpdated(long? fromOrd, long? toOrd)
        {
            var userId = await _env.CreateUser();
            var cards = await _env.CreateCards(userId, _bibleId, CancellationToken.None);

            cards = cards.Where(c => c.Tab == Common.Tab.Queue).ToList();

            if (!fromOrd.HasValue)
                fromOrd = cards.Max(c => c.RowNumber);

            if (!toOrd.HasValue)
                toOrd = cards.Max(c => c.RowNumber);

            var cardToReorder = cards.Single(c => c.RowNumber == fromOrd.Value);

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                await svc.ReorderCard(userId, Common.Tab.Queue, cardToReorder.Id, toOrd.Value, CancellationToken.None);

                var reorderedCards = await dbCtx.Cards.Where(c => c.UserId == userId && c.Tab == Common.Tab.Queue).OrderBy(c => c.NumberedCard.RowNumber).ToListAsync(CancellationToken.None);
                var updatedCard = reorderedCards.Single(c => c.NumberedCard.RowNumber == toOrd.Value);

                updatedCard.Id.Should().Be(cardToReorder.Id);

                var ord = 0;
                foreach (var card in reorderedCards)
                {
                    ord++;
                    card.NumberedCard.RowNumber.Should().Be(ord);
                }
            }
        }
    }

    
}

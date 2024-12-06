using BitPantry.Iota.Application;
using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Test.Fixtures;
using Dapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BitPantry.Iota.Test.ServiceTests
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

                resp.Result.Should().Be(Application.CreateCardResponseResult.Ok);
                
                resp.Card.Should().NotBeNull();
                resp.Card.Address.Should().NotBeNullOrEmpty();
                resp.Card.AddedOn.Date.Should().Be(DateTime.UtcNow.Date);
                resp.Card.LastMovedOn.Date.Should().Be(DateTime.UtcNow.Date);
                resp.Card.LastReviewedOn.Should().BeNull();
                resp.Card.Order.Should().BeGreaterThan(0);
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

                resp.Result.Should().Be(Application.CreateCardResponseResult.InvalidAddress);
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

                var resp1 = await svc.CreateCard(userId, _bibleId, "gen 12:1", Common.Tab.Queue, null, CancellationToken.None);
                var resp2 = await svc.CreateCard(userId, _bibleId, "gen 12:2", Common.Tab.Queue, null, CancellationToken.None);
                var resp3 = await svc.CreateCard(userId, _bibleId, "gen 12:3", Common.Tab.Queue, null, CancellationToken.None);

                resp1.Card.Order.Should().Be(1);
                resp2.Card.Order.Should().Be(2);
                resp3.Card.Order.Should().Be(3);
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

                resp.Result.Should().Be(Application.CreateCardResponseResult.BookNameUnresolved);
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
                resp.Result.Should().Be(Application.CreateCardResponseResult.Ok);

                resp = await svc.CreateCard(userId, _bibleId, "jn 3:16", CancellationToken.None);
                resp.Result.Should().Be(Application.CreateCardResponseResult.CardAlreadyExists);
            }
        }

        [Fact]
        public async Task CreateCardWithOrder_CardCreated()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await svc.CreateCard(userId, _bibleId, "rev 1:1", Common.Tab.Day10, 1, CancellationToken.None);
                resp.Result.Should().Be(Application.CreateCardResponseResult.Ok);

                var card = await svc.GetCard(resp.Card.Id, CancellationToken.None);

                card.Order.Should().Be(1);
            }
        }

        [Fact]
        public async Task CreateCardsWithDuplicateOrder_InvalidOperationException()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp1 = await svc.CreateCard(userId, _bibleId, "rev 1:2", Common.Tab.Day10, 1, CancellationToken.None);
                resp1.Result.Should().Be(Application.CreateCardResponseResult.Ok);

                var act = async () => await svc.CreateCard(userId, _bibleId, "rev 1:3", Common.Tab.Day10, 1, CancellationToken.None);
                await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Order 1 is already taken");                
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

                await svc.MarkAsReviewed(userId, resp.Card.Tab, resp.Card.Order, CancellationToken.None);

                var card = await svc.GetCard(resp.Card.Id, CancellationToken.None);
                card.LastReviewedOn.Value.Date.Should().Be(DateTime.UtcNow.Date);
            }
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(null, 1)]
        [InlineData(1, null)]
        [InlineData(2, 1)]
        [InlineData(3, 6)]
        [InlineData(6, 1)]
        [InlineData(null, null)]
        [InlineData(1, 1)]
        public async Task ReorderCard_CardReorderedAndTabOrderUpdated(int? fromOrd, int? toOrd)
        {
            var userId = await _env.CreateUser();
            var cards = await _env.CreateCards(userId, _bibleId, CancellationToken.None);

            cards = cards.Where(c => c.Tab == Common.Tab.Queue).ToList();

            if (!fromOrd.HasValue)
                fromOrd = cards.Max(c => c.Order);

            if (!toOrd.HasValue)
                toOrd = cards.Max(c => c.Order);

            var cardToReorder = cards.Single(c => c.Order == fromOrd.Value);

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                await svc.ReorderCard(userId, Common.Tab.Queue, cardToReorder.Id, toOrd.Value, CancellationToken.None);

                var reorderedCards = await dbCtx.Cards.Where(c => c.UserId == userId && c.Tab == Common.Tab.Queue).OrderBy(c => c.Order).ToListAsync(CancellationToken.None);
                var updatedCard = reorderedCards.Single(c => c.Order == toOrd.Value);

                updatedCard.Id.Should().Be(cardToReorder.Id);

                var ord = 0;
                foreach (var card in reorderedCards)
                {
                    ord++;
                    card.Order.Should().Be(ord);
                }
            }
        }
    }

    
}

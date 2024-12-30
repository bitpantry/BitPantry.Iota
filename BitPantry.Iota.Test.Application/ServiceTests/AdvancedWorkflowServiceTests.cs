using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Test.Application.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BitPantry.Iota.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Application.ServiceTests
{
    [Collection("env")]
    public class AdvancedWorkflowServiceTests
    {
        private readonly long _bibleId;
        private readonly ApplicationEnvironment _env;

        public AdvancedWorkflowServiceTests(AppEnvironmentFixture fixture)
        {
            _bibleId = fixture.BibleId;
            _env = fixture.Environment;
        }

        [Fact]
        public async Task DeleteDailyCardFromTwo_CardDeleted()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                await svc.CreateCard(userId, _bibleId, "rom 1:16", Common.Tab.Daily, CancellationToken.None);
                await svc.CreateCard(userId, _bibleId, "rom 1:17", Common.Tab.Daily, CancellationToken.None);

                var cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).OrderBy(c => c.Tab).ToList();

                cards.Should().HaveCount(2);
                cards[0].Tab.Should().Be(Common.Tab.Daily);
                cards[1].Tab.Should().Be(Common.Tab.Daily);

                var dailyCard1 = cards[0].Id;
                var dailyCard2 = cards[1].Id;

                await wfSvc.DeleteCard(dailyCard1, CancellationToken.None);

                cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).OrderBy(c => c.Tab).ToList();

                cards.Should().HaveCount(1);
                cards[0].Tab.Should().Be(Common.Tab.Daily);
            }
        }

        [Theory]
        [InlineData(Tab.Queue, 3, Tab.Daily, 0, 1, true)] // top queue card to empty daily tab
        [InlineData(Tab.Queue, 3, Tab.Daily, 1, 1, true)] // top queue card to daily tab with one card already
        [InlineData(Tab.Queue, 2, Tab.Daily, 3, 1, true)] // second queue card to daily tab with three cards
        [InlineData(Tab.Daily, 2, Tab.Queue, 2, 1, false)] // top daily card to bottom of queue - daily has more than one card
        [InlineData(Tab.Daily, 1, Tab.Queue, 2, 1, true)] // only daily card to top of queue
        [InlineData(Tab.Daily, 1, Tab.Queue, 2, 1, false)] // only daily card to bottom of queue
        [InlineData(Tab.Daily, 1, Tab.Day10, 10, 1, false)] // only daily card to bottom of day 10 tab
        [InlineData(Tab.Day10, 10, Tab.Daily, 0, 10, false)] // bottom day 10 card to empty daily tab
        [InlineData(Tab.Day10, 10, Tab.Daily, 3, 10, false)] // bottom day 10 card to bottom of daily tab with 3
        [InlineData(Tab.Queue, 3, Tab.Odd, 2, 2, false)] // second in queue to bottom of odd tab with 2
        public async Task MoveCard_CardMoved(Tab fromTab, int numCardsInFromTab, Tab toTab, int numCardsInToTab, int moveFromRowNum, bool toTop)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                for (int i = 1; i <= numCardsInFromTab; i++)
                    await svc.CreateCard(userId, _bibleId, $"rom 1:{i}", fromTab);

                for (int i = 1; i <= numCardsInToTab; i++)
                    await svc.CreateCard(userId, _bibleId, $"rom 2:{i}", toTab);

                var cardToMove = await svc.GetCard(userId, fromTab, moveFromRowNum);

                await wfSvc.MoveCard(cardToMove.Id, toTab, toTop, CancellationToken.None);

                cardToMove = await svc.GetCard(cardToMove.Id);

                cardToMove.Tab.Should().Be(toTab);
                if (toTop)
                    cardToMove.RowNumber.Should().Be(1);
                else
                    cardToMove.RowNumber.Should().Be(numCardsInToTab + 1);

                var validateOrderAction = (Tab tab, long userId) =>
                {
                    var cards = dbCtx.Cards.Where(c => c.Tab == fromTab && c.UserId == userId).OrderBy(c => c.FractionalOrder);

                    var rowNum = 0L;
                    foreach (var item in cards)
                    {
                        item.NumberedCard.RowNumber.Should().Be(rowNum + 1);
                        rowNum = item.NumberedCard.RowNumber;
                    }
                };

                validateOrderAction(fromTab, userId);
                validateOrderAction(toTab, userId);
            }
        }

        [Fact]
        public async Task PromoteDailyCardToEmptyOddEven_PromotedToOdd()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                var dailyCard = await svc.CreateCard(userId, _bibleId, "rom 1:1", Tab.Daily);

                await wfSvc.PromoteCard(dailyCard.Card.Id, CancellationToken.None);

                var card = await svc.GetCard(dailyCard.Card.Id);

                card.Tab.Should().Be(Tab.Odd);
            }
        }

        [Fact]
        public async Task PromoteDailyCardToOddEvenWithOdd_PromotedToEven()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                var dailyCard = await svc.CreateCard(userId, _bibleId, "rom 1:1", Tab.Daily);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:2", Tab.Odd);

                await wfSvc.PromoteCard(dailyCard.Card.Id, CancellationToken.None);

                var card = await svc.GetCard(dailyCard.Card.Id);

                card.Tab.Should().Be(Tab.Even);
            }
        }

        [Fact]
        public async Task PromoteDailyCardToOddEvenWithOlderOdd_PromotedToOdd()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                var dailyCard = await svc.CreateCard(userId, _bibleId, "rom 1:1", Tab.Daily);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:2", Tab.Odd);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:3", Tab.Even);

                await wfSvc.PromoteCard(dailyCard.Card.Id, CancellationToken.None);

                var card = await svc.GetCard(dailyCard.Card.Id);

                card.Tab.Should().Be(Tab.Odd);
            }
        }

        [Fact]
        public async Task PromoteDailyCardToOddEvenWithOlderEven_PromotedToEven()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                var dailyCard = await svc.CreateCard(userId, _bibleId, "rom 1:1", Tab.Daily);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:2", Tab.Even);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:3", Tab.Odd);

                await wfSvc.PromoteCard(dailyCard.Card.Id, CancellationToken.None);

                var card = await svc.GetCard(dailyCard.Card.Id);

                card.Tab.Should().Be(Tab.Even);
            }
        }

        [Fact]
        public async Task PromoteDailyCardToOddEvenWithMoreOdd_PromotedToEven()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                var dailyCard = await svc.CreateCard(userId, _bibleId, "rom 1:1", Tab.Daily);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:4", Tab.Even);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:2", Tab.Odd);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:3", Tab.Odd);

                await wfSvc.PromoteCard(dailyCard.Card.Id, CancellationToken.None);

                var card = await svc.GetCard(dailyCard.Card.Id);

                card.Tab.Should().Be(Tab.Even);
            }
        }

        [Fact]
        public async Task PromoteDailyCardToOddEvenWithMultipleWithOlderOdd_PromotedToOdd()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                var dailyCard = await svc.CreateCard(userId, _bibleId, "rom 1:1", Tab.Daily);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:2", Tab.Odd);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:3", Tab.Odd);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:4", Tab.Even);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:5", Tab.Even);


                await wfSvc.PromoteCard(dailyCard.Card.Id, CancellationToken.None);

                var card = await svc.GetCard(dailyCard.Card.Id);

                card.Tab.Should().Be(Tab.Odd);
            }
        }

        [Theory]
        [InlineData(Tab.Queue, Tab.Daily)]
        [InlineData(Tab.Daily, Tab.Odd)]
        [InlineData(Tab.Odd, Tab.Sunday)]
        [InlineData(Tab.Even, Tab.Sunday)]
        [InlineData(Tab.Sunday, Tab.Day1)]
        [InlineData(Tab.Monday, Tab.Day1)]
        [InlineData(Tab.Tuesday, Tab.Day1)]
        [InlineData(Tab.Wednesday, Tab.Day1)]

        public async Task PromoteCard_CardPromoted(Tab fromTab, Tab expectedToTab)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                var card = await svc.CreateCard(userId, _bibleId, "rom 1:1", fromTab);

                await wfSvc.PromoteCard(card.Card.Id, CancellationToken.None);

                var promotedCard = await svc.GetCard(card.Card.Id);

                promotedCard.Tab.Should().Be(expectedToTab);
            }
        }

        [Theory]
        [InlineData(Tab.Queue, Tab.Daily)]
        [InlineData(Tab.Daily, Tab.Odd)]
        [InlineData(Tab.Odd, Tab.Sunday)]
        [InlineData(Tab.Even, Tab.Sunday)]
        [InlineData(Tab.Sunday, Tab.Day1)]
        [InlineData(Tab.Monday, Tab.Day1)]
        [InlineData(Tab.Tuesday, Tab.Day1)]
        [InlineData(Tab.Wednesday, Tab.Day1)]
        public async Task PromoteCard_ReviewCountReset(Tab fromTab, Tab toTab)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                var resp = await svc.CreateCard(userId, _bibleId, "rom 1:1", fromTab);

                resp.Card.ReviewCount.Should().Be(0);

                await svc.MarkAsReviewed(userId, resp.Card.Tab, resp.Card.RowNumber);
                
                var card = await svc.GetCard(resp.Card.Id);

                card.ReviewCount.Should().Be(1);    

                await wfSvc.PromoteCard(resp.Card.Id, CancellationToken.None);

                var promotedCard = await svc.GetCard(resp.Card.Id);

                promotedCard.Tab.Should().Be(toTab);
                promotedCard.ReviewCount.Should().Be(0);
            }
        }

        [Fact]

        public async Task PromoteDateCard_Error()
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                var card = await svc.CreateCard(userId, _bibleId, "rom 1:1", Tab.Day1);

                var func = async () => await wfSvc.PromoteCard(card.Card.Id, CancellationToken.None);

                await func.Should().ThrowAsync<ArgumentOutOfRangeException>().WithMessage("No promotion path is defined for this tab (Parameter 'currentTab')\r\nActual value was Day1.");
            }
        }

        [Theory]
        [InlineData(3, 0, 1)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 1)]
        [InlineData(3, 3, 2)]
        [InlineData(3, 1, 3)]

        public async Task StartTopQueueCard_CardStarted(int numQueueCards, int numDailyCards, int queueOrdToStart)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                scope.UseAdvancedWorkflow();

                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                for (int i = 1; i <= numQueueCards; i++)
                    _ = await svc.CreateCard(userId, _bibleId, $"rom 1:{i}", Tab.Queue);

                for (int i = 1; i <= numDailyCards; i++)
                    _ = await svc.CreateCard(userId, _bibleId, $"rom 2:{i}", Tab.Daily);

                var cardToMove = await svc.GetCard(userId, Tab.Queue, queueOrdToStart);

                await wfSvc.StartQueueCard(cardToMove.Id, CancellationToken.None);

                var movedCard = await svc.GetCard(cardToMove.Id);
                movedCard.Tab.Should().Be(Tab.Daily);
                movedCard.ReviewCount.Should().Be(0);

                var dailyCards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == Tab.Daily).ToList();
                var queueCards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == Tab.Queue).ToList();

                dailyCards.Should().HaveCount(numDailyCards + 1);
                queueCards.Should().HaveCount(numQueueCards - 1);
            }
        }
    }
}

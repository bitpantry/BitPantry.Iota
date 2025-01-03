using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Test.Application.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BitPantry.Tabs.Test.Application.ServiceTests
{
    [Collection("env")]
    public class BasicWorkflowServiceTests
    {
        private readonly long _bibleId;
        private readonly ApplicationEnvironment _env;

        public BasicWorkflowServiceTests(AppEnvironmentFixture fixture)
        {
            _bibleId = fixture.BibleId;
            _env = fixture.Environment;
        }

        [Fact]
        public async Task DeleteLastDailyEmptyQueue_CardDeleted()
        {
            var newUserId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                await svc.CreateCard(newUserId, _bibleId, "rom 1:16", Common.Tab.Daily, CancellationToken.None);

                var cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == newUserId).OrderBy(c => c.Tab).ToList();

                cards.Should().HaveCount(1);
                cards[0].Tab.Should().Be(Common.Tab.Daily);

                var dailyCard = cards[0].Id;

                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();

                await wfSvc.DeleteCard(dailyCard, CancellationToken.None);

                cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == newUserId).OrderBy(c => c.Tab).ToList();

                cards.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task MoveQueueCardToOccupiedDaily_CardMovedCascadingToOdd()
        {
            var newUserId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var respQueue = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:1", Tab.Queue, CancellationToken.None);
                var respDaily = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:2", Tab.Daily, CancellationToken.None);

                await wfSvc.MoveCard(respQueue.Card.Id, Tab.Daily, true, CancellationToken.None);

                var queueCard = await cardSvc.GetCard(respQueue.Card.Id, CancellationToken.None);
                queueCard.Tab.Should().Be(Tab.Daily);

                var dailyCard = await cardSvc.GetCard(respDaily.Card.Id, CancellationToken.None);
                dailyCard.Tab.Should().Be(Tab.Odd);
            }
        }

        [Fact]
        public async Task MoveQueueCardToOccupiedDailyOdd_CardMovedCascadingToEven()
        {
            var newUserId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var respQueue = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:1", Tab.Queue, CancellationToken.None);
                var respDaily = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:2", Tab.Daily, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:3", Tab.Odd, CancellationToken.None);

                await wfSvc.MoveCard(respQueue.Card.Id, Tab.Daily, true, CancellationToken.None);

                var queueCard = await cardSvc.GetCard(respQueue.Card.Id, CancellationToken.None);
                queueCard.Tab.Should().Be(Tab.Daily);

                var dailyCard = await cardSvc.GetCard(respDaily.Card.Id, CancellationToken.None);
                dailyCard.Tab.Should().Be(Tab.Even);
            }
        }

        [Fact]
        public async Task MoveQueueCardToOccupiedDailyOddEven_CardMovedCascading()
        {
            var newUserId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var respQueue = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:1", Tab.Queue, CancellationToken.None);
                var respDaily = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:2", Tab.Daily, CancellationToken.None);
                var respOdd = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:3", Tab.Odd, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:4", Tab.Even, CancellationToken.None);

                await wfSvc.MoveCard(respQueue.Card.Id, Tab.Daily, true, CancellationToken.None);

                var queueCard = await cardSvc.GetCard(respQueue.Card.Id, CancellationToken.None);
                queueCard.Tab.Should().Be(Tab.Daily);

                var dailyCard = await cardSvc.GetCard(respDaily.Card.Id, CancellationToken.None);
                dailyCard.Tab.Should().Be(Tab.Odd);

                var oddCard = await cardSvc.GetCard(respOdd.Card.Id, CancellationToken.None);
                oddCard.Tab.Should().Be(Tab.Sunday);
            }
        }

        [Fact]
        public async Task MoveQueueCardToOccupiedDailyOddEvenSunday_CardMovedCascading()
        {
            var newUserId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var respQueue = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:1", Tab.Queue, CancellationToken.None);
                var respDaily = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:2", Tab.Daily, CancellationToken.None);
                var respOdd = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:3", Tab.Odd, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:4", Tab.Even, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:5", Tab.Sunday, CancellationToken.None);

                await wfSvc.MoveCard(respQueue.Card.Id, Tab.Daily, true, CancellationToken.None);

                var queueCard = await cardSvc.GetCard(respQueue.Card.Id, CancellationToken.None);
                queueCard.Tab.Should().Be(Tab.Daily);

                var dailyCard = await cardSvc.GetCard(respDaily.Card.Id, CancellationToken.None);
                dailyCard.Tab.Should().Be(Tab.Odd);

                var oddCard = await cardSvc.GetCard(respOdd.Card.Id, CancellationToken.None);
                oddCard.Tab.Should().Be(Tab.Monday);
            }
        }

        [Fact]
        public async Task MoveQueueCardToOccupiedDailyOddEvenWeek_CardMovedCascading()
        {
            var newUserId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var respQueue = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:1", Tab.Queue, CancellationToken.None);

                var respDaily = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:2", Tab.Daily, CancellationToken.None);

                var respOdd = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:3", Tab.Odd, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:4", Tab.Even, CancellationToken.None);

                var respTuesday = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:5", Tab.Tuesday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:6", Tab.Sunday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:7", Tab.Monday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:8", Tab.Wednesday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:9", Tab.Thursday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:10", Tab.Friday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:11", Tab.Saturday, CancellationToken.None);

                await wfSvc.MoveCard(respQueue.Card.Id, Tab.Daily, true, CancellationToken.None);

                var queueCard = await cardSvc.GetCard(respQueue.Card.Id, CancellationToken.None);
                queueCard.Tab.Should().Be(Tab.Daily);

                var dailyCard = await cardSvc.GetCard(respDaily.Card.Id, CancellationToken.None);
                dailyCard.Tab.Should().Be(Tab.Odd);

                var oddCard = await cardSvc.GetCard(respOdd.Card.Id, CancellationToken.None);
                oddCard.Tab.Should().Be(Tab.Tuesday);

                var tuesdayCard = await cardSvc.GetCard(respTuesday.Card.Id, CancellationToken.None);
                tuesdayCard.Tab.Should().Be(Tab.Day1);
            }
        }

        [Fact]
        public async Task MoveQueueCardToOccupiedDailyOddEvenWeekDay1_CardMovedCascading()
        {
            var newUserId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var respQueue = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:1", Tab.Queue, CancellationToken.None);

                var respDaily = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:2", Tab.Daily, CancellationToken.None);

                var respOdd = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:3", Tab.Odd, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:4", Tab.Even, CancellationToken.None);

                var respTuesday = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:5", Tab.Tuesday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:6", Tab.Sunday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:7", Tab.Monday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:8", Tab.Wednesday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:9", Tab.Thursday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:10", Tab.Friday, CancellationToken.None);
                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:11", Tab.Saturday, CancellationToken.None);

                _ = await cardSvc.CreateCard(newUserId, _bibleId, "gen 1:12", Tab.Day1, CancellationToken.None);

                await wfSvc.MoveCard(respQueue.Card.Id, Tab.Daily, true, CancellationToken.None);

                var queueCard = await cardSvc.GetCard(respQueue.Card.Id, CancellationToken.None);
                queueCard.Tab.Should().Be(Tab.Daily);

                var dailyCard = await cardSvc.GetCard(respDaily.Card.Id, CancellationToken.None);
                dailyCard.Tab.Should().Be(Tab.Odd);

                var oddCard = await cardSvc.GetCard(respOdd.Card.Id, CancellationToken.None);
                oddCard.Tab.Should().Be(Tab.Tuesday);

                var tuesdayCard = await cardSvc.GetCard(respTuesday.Card.Id, CancellationToken.None);
                tuesdayCard.Tab.Should().Be(Tab.Day2);
            }
        }

        [Fact]
        public async Task MoveQueueCardToOccupiedDailyOddEvenWeekDate_CardMovedCascading()
        {
            var newUserId = await _env.CreateUser();
            await _env.CreateCards(newUserId, _bibleId, CancellationToken.None);

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var tabSvc = scope.ServiceProvider.GetRequiredService<TabsService>();

                var dailyCard = await cardSvc.GetCard(newUserId, Tab.Daily, 1, CancellationToken.None);
                var oddCard = await cardSvc.GetCard(newUserId, Tab.Odd, 1, CancellationToken.None);
                var sundayCard = await cardSvc.GetCard(newUserId, Tab.Sunday, 1, CancellationToken.None);

                var queueResp = await cardSvc.CreateCard(newUserId, _bibleId, "gen 6:1", CancellationToken.None);

                await wfSvc.MoveCard(queueResp.Card.Id, Tab.Daily, true, CancellationToken.None);

                dailyCard = await cardSvc.GetCard(dailyCard.Id, CancellationToken.None);
                oddCard = await cardSvc.GetCard(oddCard.Id, CancellationToken.None);
                sundayCard = await cardSvc.GetCard(sundayCard.Id, CancellationToken.None);

                dailyCard.Tab.Should().Be(Tab.Odd);
                oddCard.Tab.Should().Be(Tab.Sunday);
                sundayCard.Tab.Should().Be(Tab.Day1);

                var cards = await tabSvc.GetCardsForTab(newUserId, Tab.Day1, CancellationToken.None);

                cards.Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task MoveQueueCardToOccupiedDailyOddEvenWeekDateDay1_CardMovedCascading()
        {
            var newUserId = await _env.CreateUser();
            await _env.CreateCards(newUserId, _bibleId, CancellationToken.None);

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var tabSvc = scope.ServiceProvider.GetRequiredService<TabsService>();

                var dailyCard = await cardSvc.GetCard(newUserId, Tab.Daily, 1, CancellationToken.None);
                var oddCard = await cardSvc.GetCard(newUserId, Tab.Odd, 1, CancellationToken.None);
                var sundayCard = await cardSvc.GetCard(newUserId, Tab.Sunday, 1, CancellationToken.None);

                var queueResp = await cardSvc.CreateCard(newUserId, _bibleId, "gen 6:1", CancellationToken.None);
                var day1Resp = await cardSvc.CreateCard(newUserId, _bibleId, "gen 6:2", Tab.Day1, CancellationToken.None);

                await wfSvc.MoveCard(queueResp.Card.Id, Tab.Daily, true, CancellationToken.None);

                dailyCard = await cardSvc.GetCard(dailyCard.Id, CancellationToken.None);
                oddCard = await cardSvc.GetCard(oddCard.Id, CancellationToken.None);
                sundayCard = await cardSvc.GetCard(sundayCard.Id, CancellationToken.None);

                dailyCard.Tab.Should().Be(Tab.Odd);
                oddCard.Tab.Should().Be(Tab.Sunday);
                sundayCard.Tab.Should().Be(Tab.Day2);

                var cards = await tabSvc.GetCardsForTab(newUserId, Tab.Day1, CancellationToken.None);

                cards.Should().HaveCount(2);
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
    }
}

using BitPantry.Iota.Application;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Test.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BitPantry.Iota.Test.ServiceTests
{
    [Collection("Services")]
    public class BasicWorkflowServiceTests
    {
        private readonly long _bibleId;
        private readonly ApplicationEnvironment _testEnv;

        public BasicWorkflowServiceTests(ApplicationEnvironmentCollectionFixture fixture)
        {
            _bibleId = fixture.BibleId;
            _testEnv = fixture.Environment;
        }

        [Fact]
        public async Task DeleteDailyCard_QueueCardAutoPromoted()
        {
            var newUserId = await _testEnv.CreateUser();
            long dailyCardId = 0;
            long queueCardId = 0;

            using (var scope = _testEnv.CreateDependencyScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                await svc.CreateCard(newUserId, _bibleId, "rom 1:16", Common.Tab.Queue, CancellationToken.None);
                await svc.CreateCard(newUserId, _bibleId, "rom 1:17", Common.Tab.Daily, CancellationToken.None);

                var cards = dbCtx.Cards.Where(c => c.UserId == newUserId).OrderBy(c => c.Tab).ToList();

                cards.Should().HaveCount(2);
                cards[0].Tab.Should().Be(Common.Tab.Queue);
                cards[1].Tab.Should().Be(Common.Tab.Daily);

                queueCardId = cards[0].Id;
                dailyCardId = cards[1].Id;
            }

            using (var scope = _testEnv.CreateDependencyScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                await svc.DeleteCard(dailyCardId, CancellationToken.None);

                var cards = dbCtx.Cards.Where(c => c.UserId == newUserId).OrderBy(c => c.Tab).ToList();

                cards.Should().HaveCount(1);
                cards[0].Tab.Should().Be(Common.Tab.Daily);
                cards[0].Id.Should().Be(queueCardId);
            }
        }

        [Fact]
        public async Task MoveQueueCardToOccupiedDaily_CardMovedCascadingToOdd()
        {
            var newUserId = await _testEnv.CreateUser();

            using (var scope = _testEnv.CreateDependencyScope())
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
            var newUserId = await _testEnv.CreateUser();

            using (var scope = _testEnv.CreateDependencyScope())
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
            var newUserId = await _testEnv.CreateUser();

            using (var scope = _testEnv.CreateDependencyScope())
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
            var newUserId = await _testEnv.CreateUser();

            using (var scope = _testEnv.CreateDependencyScope())
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
            var newUserId = await _testEnv.CreateUser();

            using (var scope = _testEnv.CreateDependencyScope())
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
            var newUserId = await _testEnv.CreateUser();

            using (var scope = _testEnv.CreateDependencyScope())
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
            var newUserId = await _testEnv.CreateUser();
            await _testEnv.CreateCards(newUserId, _bibleId, CancellationToken.None);

            using (var scope = _testEnv.CreateDependencyScope())
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
            var newUserId = await _testEnv.CreateUser();
            await _testEnv.CreateCards(newUserId, _bibleId, CancellationToken.None);

            using (var scope = _testEnv.CreateDependencyScope())
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


    }
}

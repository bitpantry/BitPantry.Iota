using BitPantry.Iota.Application;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Test.Application.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BitPantry.Iota.Test;
using BitPantry.Iota.Application.DTO;

namespace BitPantry.Iota.Test.Application.ServiceTests
{
    [Collection("env")]
    public class CommonWorkflowServiceTests 
    {
        private readonly ApplicationEnvironment _env;
        private long _bibleId;

        public CommonWorkflowServiceTests(AppEnvironmentFixture fixture)
        {
            _env = fixture.Environment;
            _bibleId = fixture.BibleId;
        }


        [Theory]
        [InlineData(WorkflowType.Basic)]
        [InlineData(WorkflowType.Advanced)]
        public async Task GetReviewPathFullData_ReviewPathCreated(WorkflowType workflowType)
        {
            var userId = await _env.CreateUser();
            await _env.CreateCards(userId, _bibleId);

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetWorkflowService(workflowType);

                var path = await svc.GetReviewPath(userId, DateTime.Parse("12/1/2024"), CancellationToken.None);

                path.UserId.Should().Be(userId);
                path.Path.Count.Should().Be(4);

                path.Path.ToList()[0].Key.Should().Be(Tab.Daily);
                path.Path.ToList()[0].Value.Should().Be(1);

                path.Path.ToList()[1].Key.Should().Be(Tab.Odd);
                path.Path.ToList()[1].Value.Should().Be(1);

                path.Path.ToList()[2].Key.Should().Be(Tab.Sunday);
                path.Path.ToList()[2].Value.Should().Be(1);

                path.Path.ToList()[3].Key.Should().Be(Tab.Day1);
                path.Path.ToList()[3].Value.Should().Be(1);
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        [InlineData(WorkflowType.Advanced)]
        public async Task GetReviewPathPartialData_ReviewPathCreated(WorkflowType workflowType)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                await cardSvc.CreateCard(userId, _bibleId, "gen 1:1", Tab.Daily, CancellationToken.None);
                await cardSvc.CreateCard(userId, _bibleId, "gen 1:2", Tab.Odd, CancellationToken.None);
                await cardSvc.CreateCard(userId, _bibleId, "gen 1:3", Tab.Day1, CancellationToken.None);
                await cardSvc.CreateCard(userId, _bibleId, "gen 1:4", Tab.Day1, CancellationToken.None);

                var path = await wfSvc.GetReviewPath(userId, DateTime.Parse("12/1/2024"), CancellationToken.None);

                path.UserId.Should().Be(userId);
                path.Path.Count.Should().Be(4);

                path.Path.ToList()[0].Key.Should().Be(Tab.Daily);
                path.Path.ToList()[0].Value.Should().Be(1);

                path.Path.ToList()[1].Key.Should().Be(Tab.Odd);
                path.Path.ToList()[1].Value.Should().Be(1);

                path.Path.ToList()[2].Key.Should().Be(Tab.Sunday);
                path.Path.ToList()[2].Value.Should().Be(0);

                path.Path.ToList()[3].Key.Should().Be(Tab.Day1);
                path.Path.ToList()[3].Value.Should().Be(2);
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        [InlineData(WorkflowType.Advanced)]
        public async Task DeleteOnlyDailyEmptyQueue_CardDeleted(WorkflowType workflowType)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);


                await svc.CreateCard(userId, _bibleId, "rom 1:16", Common.Tab.Daily, CancellationToken.None);

                var cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).OrderBy(c => c.Tab).ToList();

                cards.Should().HaveCount(1);
                cards[0].Tab.Should().Be(Common.Tab.Daily);

                var dailyCard = cards[0].Id;

                await wfSvc.DeleteCard(dailyCard, CancellationToken.None);

                cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).OrderBy(c => c.Tab).ToList();

                cards.Should().BeEmpty();
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        [InlineData(WorkflowType.Advanced)]
        public async Task DeleteOnlyDailyWithQueue_CardDeletedNoQueuePromoted(WorkflowType workflowType)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);

                var dailyCard = await svc.CreateCard(userId, _bibleId, "rom 1:16", Common.Tab.Daily, CancellationToken.None);
                var queueCard = await svc.CreateCard(userId, _bibleId, "rom 1:17", Common.Tab.Queue, CancellationToken.None);

                await wfSvc.DeleteCard(dailyCard.Card.Id, CancellationToken.None);

                var cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).OrderBy(c => c.Tab).ToList();

                cards.Should().HaveCount(1);
                cards[0].Id.Should().Be(queueCard.Card.Id);
                cards[0].Tab.Should().Be(Common.Tab.Queue);
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        [InlineData(WorkflowType.Advanced)]
        public async Task GetReviewPathNoData_EmptyReviewPathCreated(WorkflowType workflowType)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var path = await wfSvc.GetReviewPath(userId, DateTime.Parse("12/1/2024"), CancellationToken.None);

                path.UserId.Should().Be(userId);
                path.Path.Count.Should().Be(4);
                foreach (var key in path.Path.Keys)
                    path.Path[key].Should().Be(0);
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic, 0)]
        [InlineData(WorkflowType.Basic, 5)]
        [InlineData(WorkflowType.Basic, null)] // last
        [InlineData(WorkflowType.Advanced, 0)]
        [InlineData(WorkflowType.Advanced, 5)]
        [InlineData(WorkflowType.Advanced, null)] // last
        public async Task DeleteQueueCard_CardDeletedTabReordered(WorkflowType workflowType, int? cardIndex)
        {
            var userId = await _env.CreateUser();

            var cardDtos = await _env.CreateCards(userId, _bibleId);
            cardDtos = cardDtos.Where(c => c.Tab == Common.Tab.Queue).ToList();

            var cardToDelete = cardIndex.HasValue ? cardDtos[cardIndex.Value] : cardDtos.Last();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                await svc.DeleteCard(cardToDelete.Id, CancellationToken.None);

                var cards = dbCtx.Cards.Where(c => c.UserId == userId && c.Tab == Common.Tab.Queue);

                cards.Count().Should().Be(cardDtos.Count - 1);
                cards.Where(c => c.Id == cardToDelete.Id).FirstOrDefault().Should().BeNull();

                var rowNum = 0;
                foreach (var card in cards)
                {
                    rowNum++;
                    card.NumberedCard.RowNumber.Should().Be(rowNum);
                }
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        [InlineData(WorkflowType.Advanced)]
        public async Task DeleteLastCardInTab_CardDeleted(WorkflowType workflowType)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var resp = await svc.CreateCard(userId, _bibleId, "rom 1:16", CancellationToken.None);

                await wfSvc.DeleteCard(resp.Card.Id, CancellationToken.None);
                var cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == resp.Card.Tab).ToList();

                cards.Should().BeEmpty();
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        [InlineData(WorkflowType.Advanced)]
        public async Task MoveOnlyDailyCardToEmptyQueue_QueueCardNotPromoted(WorkflowType workflowType)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);

                var resp = await svc.CreateCard(userId, _bibleId, "rom 1:16", Tab.Daily, CancellationToken.None);

                await wfSvc.MoveCard(resp.Card.Id, Tab.Queue, true, CancellationToken.None);
                var movedCard = await svc.GetCard(resp.Card.Id);

                movedCard.Tab.Should().Be(Tab.Queue);
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        [InlineData(WorkflowType.Advanced)]
        public async Task MoveOnlyDailyToMultipleQueue_NoQueueCardPromoted(WorkflowType workflowType)
        {
            var userId = await _env.CreateUser();

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var dailyCard = await svc.CreateCard(userId, _bibleId, "rom 1:16", Tab.Daily, CancellationToken.None);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:1", Tab.Queue, CancellationToken.None);
                _ = await svc.CreateCard(userId, _bibleId, "rom 1:2", Tab.Queue, CancellationToken.None);


                await wfSvc.MoveCard(dailyCard.Card.Id, Tab.Queue, true, CancellationToken.None);
                var movedCard = await svc.GetCard(dailyCard.Card.Id);

                movedCard.Tab.Should().Be(Tab.Queue);

                var queueCards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == Tab.Queue).ToListAsync();
                queueCards.Should().HaveCount(3);
            }
        }

    }
}

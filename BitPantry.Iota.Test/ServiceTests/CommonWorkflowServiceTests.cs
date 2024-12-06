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
    public class CommonWorkflowServiceTests 
    {
        private readonly ApplicationEnvironment _testEnv;
        private long _bibleId;

        public CommonWorkflowServiceTests(ApplicationEnvironmentCollectionFixture fixture)
        {
            _testEnv = fixture.Environment;
            _bibleId = fixture.BibleId;
        }


        [Theory]
        [InlineData(WorkflowType.Basic)]
        public async Task GetReviewPathFullData_ReviewPathCreated(WorkflowType workflowType)
        {
            var userId = await _testEnv.CreateUser();
            await _testEnv.CreateCards(userId, _bibleId);

            using (var scope = _testEnv.CreateDependencyScope())
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
        public async Task GetReviewPathPartialData_ReviewPathCreated(WorkflowType workflowType)
        {
            var userId = await _testEnv.CreateUser();

            using (var scope = _testEnv.CreateDependencyScope())
            {
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                await cardSvc.CreateCard(userId, _bibleId, "gen 1:1", Tab.Daily, CancellationToken.None);
                await cardSvc.CreateCard(userId, _bibleId, "gen 1:2", Tab.Odd, CancellationToken.None);
                await cardSvc.CreateCard(userId, _bibleId, "gen 1:3", Tab.Day1, CancellationToken.None);
                await cardSvc.CreateCard(userId, _bibleId, "gen 1:4", Tab.Day1, CancellationToken.None);

                var path = await wfSvc.GetReviewPath(userId, DateTime.Parse("12/1/2024"), CancellationToken.None);

                path.UserId.Should().Be(userId);
                path.Path.Count.Should().Be(3);

                path.Path.ToList()[0].Key.Should().Be(Tab.Daily);
                path.Path.ToList()[0].Value.Should().Be(1);

                path.Path.ToList()[1].Key.Should().Be(Tab.Odd);
                path.Path.ToList()[1].Value.Should().Be(1);

                path.Path.ToList()[2].Key.Should().Be(Tab.Day1);
                path.Path.ToList()[2].Value.Should().Be(2);
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        public async Task GetReviewPathNoData_EmptyReviewPathCreated(WorkflowType workflowType)
        {
            var userId = await _testEnv.CreateUser();

            using (var scope = _testEnv.CreateDependencyScope())
            {
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var path = await wfSvc.GetReviewPath(userId, DateTime.Parse("12/1/2024"), CancellationToken.None);

                path.UserId.Should().Be(userId);
                path.Path.Count.Should().Be(0);
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic, 0)]
        [InlineData(WorkflowType.Basic, 5)]
        [InlineData(WorkflowType.Basic, null)] // last
        public async Task DeleteQueueCard_CardDeletedTabReordered(WorkflowType workflowType, int? cardIndex)
        {
            var userId = await _testEnv.CreateUser();

            var cardDtos = await _testEnv.CreateCards(userId, _bibleId);
            cardDtos = cardDtos.Where(c => c.Tab == Common.Tab.Queue).ToList();

            var cardToDelete = cardIndex.HasValue ? cardDtos[cardIndex.Value] : cardDtos.Last();

            using (var scope = _testEnv.CreateDependencyScope())
            {
                var svc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                await svc.DeleteCard(cardToDelete.Id, CancellationToken.None);

                var cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == Common.Tab.Queue);

                cards.Count().Should().Be(cardDtos.Count - 1);
                cards.Where(c => c.Id == cardToDelete.Id).FirstOrDefault().Should().BeNull();

                var ord = 0;
                foreach (var card in cards)
                {
                    ord++;
                    card.Order.Should().Be(ord);
                }
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        public async Task DeleteLastCardInTab_CardDeleted(WorkflowType workflowType)
        {
            var userId = await _testEnv.CreateUser();
            CreateCardResponse resp = null;

            using (var scope = _testEnv.CreateDependencyScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<CardService>();
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                resp = await svc.CreateCard(userId, _bibleId, "rom 1:16", CancellationToken.None);

                await wfSvc.DeleteCard(resp.Card.Id, CancellationToken.None);
                var cards = dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == resp.Card.Tab).ToList();

                cards.Should().BeEmpty();
            }
        }

        [Theory]
        [InlineData(WorkflowType.Basic)]
        public async Task MoveQueueCardToEmptyDaily_CardMoved(WorkflowType workflowType)
        {
            var userId = await _testEnv.CreateUser();

            using (var scope = _testEnv.CreateDependencyScope())
            {
                var wfSvc = scope.ServiceProvider.GetWorkflowService(workflowType);
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await cardSvc.CreateCard(userId, _bibleId, "gen 1:1", Tab.Queue, CancellationToken.None);

                await wfSvc.MoveCard(resp.Card.Id, Tab.Daily, true, CancellationToken.None);

                var card = await cardSvc.GetCard(resp.Card.Id, CancellationToken.None);
                card.Tab.Should().Be(Tab.Daily);
            }
        }
    }
}

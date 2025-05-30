﻿using BitPantry.Tabs.Application;
using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;
using FluentAssertions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Test.Playwright.Workflow.Advanced
{
    [TestClass]
    public class CardMaintenanceTests : PageTest
    {
        private async Task<long> Init()
        {
            var userId = Fixture.Environment.CreateUser(WorkflowType.Advanced).GetAwaiter().GetResult();
            await Page.AuthenticateUser(userId);
            return userId;
        }

        [DataTestMethod]
        [DataRow(Tab.Queue)]
        [DataRow(Tab.Daily)]
        [DataRow(Tab.Odd)]
        [DataRow(Tab.Monday)]
        [DataRow(Tab.Day1)]
        public async Task MaintenanceView_AllElementsRight(Tab tab)
        {
            var userId = await Init();
            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonCardMaintenanceLogic.MaintenanceView_AllElementsRight(Page, scope, userId, tab, EvaluateMaintenanceView);
        }

        [DataTestMethod]
        [DataRow(Tab.Daily)]
        [DataRow(Tab.Odd)]
        [DataRow(Tab.Monday)]
        [DataRow(Tab.Day1)]
        public async Task CloseMaintenance_BackToCollectionTab(Tab tab)
        {
            var userId = await Init();
            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonCardMaintenanceLogic.CloseMaintenance_BackToCollectionTab(Page, scope, userId, tab);
        }

        [DataTestMethod]
        [DataRow(Tab.Daily, 2, 0)]
        [DataRow(Tab.Odd, 0, 0)]
        [DataRow(Tab.Monday, 1, 3)]
        [DataRow(Tab.Day1, 0, 4)]
        public async Task SendCardToQueue_CardSentToQueue(Tab tab, int addtlCardsInTab, int cardsInQueue)
        {
            var userId = await Init();
            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonCardMaintenanceLogic.SendCardToQueue_CardSentToQueue(Page, scope, userId, tab, addtlCardsInTab, cardsInQueue);
        }

        [DataTestMethod]
        [DataRow(Tab.Daily, 0)]
        [DataRow(Tab.Queue, 0)]
        [DataRow(Tab.Daily, 2)]
        [DataRow(Tab.Queue, 2)]
        [DataRow(Tab.Odd, 0)]
        [DataRow(Tab.Even, 3)]
        [DataRow(Tab.Day1, 0)]
        [DataRow(Tab.Day2, 3)]
        public async Task DeleteCard_CardDeleted(Tab tab, int addtlCardsInTab)
        {
            var userId = await Init();
            using(var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonCardMaintenanceLogic.DeleteCard_CardDeleted(Page, scope, userId, tab, addtlCardsInTab);
        }

        [DataTestMethod]
        [DataRow(1, 1)]
        [DataRow(2, 1)]
        [DataRow(2, 2)]
        public async Task MoveToDailyTabFromQueue_CardMoved(int queueCardCount, int queueOrder)
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                for (int i = 1; i <= queueCardCount; i++)
                    _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 1:{i}", Tab.Queue);

                var queueCard = await cardSvc.GetCard(userId, Tab.Queue, queueOrder);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{queueCard.Id}"));

                await Page.GetByTestId("card.maint.btnMoveToDailyTab").ClickAsync();

                queueCard = await cardSvc.GetCard(queueCard.Id);

                queueCard.Tab.Should().Be(Tab.Daily);
            }

        }

        [DataTestMethod]
        [DataRow(Tab.Daily, 1, Tab.Queue, 0)]
        [DataRow(Tab.Queue, 2, Tab.Day1, 1)]
        [DataRow(Tab.Day1, 0, Tab.Sunday, 0)]
        [DataRow(Tab.Monday, 3, Tab.Queue, 2)]
        public async Task MoveCard_CardMoved(Tab fromTab, int fromCardCount, Tab toTab, int toCardCount)
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var cardToMoveResp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", fromTab);

                if(fromCardCount > 1)
                {
                    for (int i = 0; i < fromCardCount - 1; i++)
                        _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 2:{i}", fromTab);
                }

                for (int i = 0; i < toCardCount - 1; i++)
                    _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 3:{i}", toTab);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"card/{cardToMoveResp.Card.Id}"));

                await Page.GetByTestId("card.maint.btnMove").ClickAsync();

                if(toTab > Tab.Even && toTab < Tab.Day1)
                {
                    await Page.GetByTestId("card.maint.move.weekTab").ClickAsync();
                    await Page.GetByTestId($"card.maint.move.{toTab.ToString().ToLower()}Tab").ClickAsync();
                }
                else if(toTab > Tab.Saturday)
                {
                    await Page.GetByTestId("card.maint.move.dateTab").ClickAsync();
                    await Page.GetByTestId($"card.maint.move.{toTab.ToString().ToLower()}Tab").ClickAsync();
                }
                else
                {
                    await Page.GetByTestId($"card.maint.move.{toTab.ToString().ToLower()}Tab").ClickAsync();
                }

                await Page.GetByTestId("card.maint.btnConfirmMove").ClickAsync();

                await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"card/{cardToMoveResp.Card.Id}"));

                var movedCard = await cardSvc.GetCard(cardToMoveResp.Card.Id);

                movedCard.Tab.Should().Be(toTab);
            }
        }

        [TestMethod]
        public async Task NavigateDirectlyToStartNow_Forbidden()
        {
            _ = await Init();
            var response = await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("card/startnow/0"));
            await response.FinishedAsync();
            response.Status.Should().Be(404);
        }

        public async Task EvaluateMaintenanceView(IPage page, Tab tab, string address = null, string passageContains = null)
        {
            await Expect(page.GetByTestId("card.maint.tab")).ToContainTextAsync(tab.Humanize());

            await Expect(page.GetByTestId("card.maint.btnClose")).ToBeVisibleAsync();

            await Expect(page.GetByTestId("card.maint.btnDelete")).ToBeVisibleAsync();

            await Expect(page.GetByTestId("card.maint.btnMove")).ToBeVisibleAsync();

            if (address != null)
                await Expect(page.GetByRole(AriaRole.Heading)).ToContainTextAsync(address);

            if (passageContains != null)
                await Expect(page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync(passageContains);

            await Expect(page.GetByTestId("card.maint.btnStartNow")).ToHaveCountAsync(0);

            if (tab != Tab.Queue)
            {
                await Expect(page.GetByTestId("card.maint.btnSendBackToQueue")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.maint.btnMoveToDailyTab")).ToHaveCountAsync(0);
            }
            else
            {
                await Expect(page.GetByTestId("card.maint.btnSendBackToQueue")).ToHaveCountAsync(0);
                await Expect(Page.GetByTestId("card.maint.btnMoveToDailyTab")).ToHaveCountAsync(1);
            }
        }

    }
}

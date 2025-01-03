using BitPantry.Tabs.Application;
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

namespace BitPantry.Tabs.Test.Playwright.Workflow.Basic
{
    [TestClass]
    public class CardMaintenanceTests : PageTest
    {
        private async Task<long> Init()
        {
            var userId = Fixture.Environment.CreateUser().GetAwaiter().GetResult();
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
        [DataRow(Tab.Daily, 0, 0)]
        [DataRow(Tab.Odd, 0, 0)]
        [DataRow(Tab.Monday, 0, 3)]
        [DataRow(Tab.Day1, 3, 4)]
        public async Task SendCardToQueue_CardSentToQueue(Tab tab, int addtlCardsInTab, int cardsInQueue)
        {
            var userId = await Init();
            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonCardMaintenanceLogic.SendCardToQueue_CardSentToQueue(Page, scope, userId, tab, addtlCardsInTab, cardsInQueue);
        }

        [DataTestMethod]
        [DataRow(Tab.Daily, 0)]
        [DataRow(Tab.Queue, 4)]
        [DataRow(Tab.Odd, 0)]
        [DataRow(Tab.Even, 0)]
        [DataRow(Tab.Monday, 0)]
        [DataRow(Tab.Day2, 3)]
        public async Task DeleteCard_CardDeleted(Tab tab, int addtlCardsInTab)
        {
            var userId = await Init();
            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonCardMaintenanceLogic.DeleteCard_CardDeleted(Page, scope, userId, tab, addtlCardsInTab);
        }

        [DataTestMethod]
        [DataRow(1, 1, false)]
        [DataRow(1, 1, true)]
        [DataRow(3, 1, false)]
        [DataRow(3, 2, true)]
        [DataRow(3, 3, true)]
        public async Task StartQueueCardNow_CardStarted(int queueCardCount, int queueOrder, bool isDailyCard)
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                for (int i = 1; i <= queueCardCount; i++) 
                    _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 1:{i}", Tab.Queue);

                CreateCardResponse dailyCard = isDailyCard
                    ? await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 2:1", Tab.Daily)
                    : null;

                var queueCard = await cardSvc.GetCard(userId, Tab.Queue, queueOrder);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{queueCard.Id}"));

                await Page.GetByTestId("card.maint.btnStartNow").ClickAsync();
                await Page.GetByTestId("card.maint.btnConfirmStartNow").ClickAsync();

                await Page.WaitForUrlAsyncIgnoreCase(Fixture.Environment.GetUrlBuilder().Build("review/Daily/1"));

                var dailyCardList = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == Tab.Daily).ToListAsync();
                dailyCardList.Should().HaveCount(1);
                dailyCardList.Single().Id.Should().Be(queueCard.Id);

                var queueCardList = await dbCtx.Cards.Where(c => c.UserId == userId && c.Tab == Tab.Queue).ToListAsync();

                if (isDailyCard)
                    queueCardList.Should().HaveCount(queueCardCount);
                else
                    queueCardList.Should().HaveCount(queueCardCount - 1);

                if(isDailyCard)
                    queueCardList.OrderBy(c => c.FractionalOrder).First().Id.Should().Be(dailyCard.Card.Id);

                var rowNum = 0L;
                foreach (var item in queueCardList.OrderBy(c => c.NumberedCard.RowNumber))
                {
                    rowNum.Should().Be(item.NumberedCard.RowNumber - 1);
                    rowNum = item.NumberedCard.RowNumber;
                }
            }

        }

        [TestMethod]
        public async Task NavigateDirectlyToMoveToDailyTab_Forbidden()
        {
            _ = await Init();
            var response = await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("card/moveToDailyTab/0"));
            await response.FinishedAsync();
            response.Status.Should().Be(404);
        }

        public async Task EvaluateMaintenanceView(IPage page, Tab tab, string address = null, string passageContains = null)
        {
            await Expect(page.GetByTestId("card.maint.tab")).ToContainTextAsync(tab.Humanize());

            await Expect(page.GetByTestId("card.maint.btnClose")).ToBeVisibleAsync();

            await Expect(page.GetByTestId("card.maint.btnDelete")).ToBeVisibleAsync();

            await Expect(page.GetByTestId("card.maint.btnMove")).ToHaveCountAsync(0);

            if (address != null)
                await Expect(page.GetByRole(AriaRole.Heading)).ToContainTextAsync(address);

            if (passageContains != null)
                await Expect(page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync(passageContains);

            await Expect(Page.GetByTestId("card.maint.btnMoveToDailyTab")).ToHaveCountAsync(0);

            if(tab == Tab.Queue)
                await Expect(page.GetByTestId("card.maint.btnStartNow")).ToBeVisibleAsync();
            else
                await Expect(page.GetByTestId("card.maint.btnStartNow")).ToHaveCountAsync(0);

            if (tab != Tab.Queue)
                await Expect(page.GetByTestId("card.maint.btnSendBackToQueue")).ToBeVisibleAsync();
            else
                await Expect(page.GetByTestId("card.maint.btnSendBackToQueue")).ToHaveCountAsync(0);
        }



    }
}

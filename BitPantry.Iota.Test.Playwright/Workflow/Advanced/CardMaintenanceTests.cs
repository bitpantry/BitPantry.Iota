using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
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

namespace BitPantry.Iota.Test.Playwright.Workflow.Advanced
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
        [DataRow(Tab.Daily, 0)]
        [DataRow(Tab.Queue, 0)]
        [DataRow(Tab.Daily, 2)]
        [DataRow(Tab.Queue, 2)]
        [DataRow(Tab.Odd, 0)]
        [DataRow(Tab.Even, 3)]
        [DataRow(Tab.Day1, 0)]
        [DataRow(Tab.Day2, 3)]
        public async Task Test(Tab tab, int addtlCardsInTab)
        {
            var userId = await Init();
            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var cardResp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", tab);

                for (var i = 1; i <= addtlCardsInTab; i++)
                    _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 2:{i}", tab);

                var cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == tab).ToListAsync();

                cards.Select(c => c.Id).Should().Contain(cardResp.Card.Id);
                cards.Should().HaveCount(addtlCardsInTab + 1);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"card/{cardResp.Card.Id}"));

                await Page.GetByTestId("card.maint.btnTest").ClickAsync();

                await Page.WaitForUrlAsyncIgnoreCase(Fixture.Environment.GetUrlBuilder().Build($"review/Done"));

                cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == tab).ToListAsync();

                cards.Select(c => c.Id).Should().NotContain(cardResp.Card.Id);
                cards.Should().HaveCount(addtlCardsInTab);
            }
        }

        public async Task EvaluateMaintenanceView(IPage page, Tab tab, string address = null, string passageContains = null)
        {
            await Expect(page.GetByTestId("card.maint.tab")).ToContainTextAsync(tab.Humanize());

            await Expect(page.GetByTestId("card.maint.btnClose")).ToBeVisibleAsync();

            await Expect(page.GetByTestId("card.maint.btnDelete")).ToBeVisibleAsync();

            if (address != null)
                await Expect(page.GetByRole(AriaRole.Heading)).ToContainTextAsync(address);

            if (passageContains != null)
                await Expect(page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync(passageContains);

            await Expect(page.GetByTestId("card.maint.btnStartNow")).ToHaveCountAsync(0);

            if (tab != Tab.Queue)
                await Expect(page.GetByTestId("card.maint.btnSendBackToQueue")).ToBeVisibleAsync();
            else
                await Expect(page.GetByTestId("card.maint.btnSendBackToQueue")).ToHaveCountAsync(0);
        }
    }
}

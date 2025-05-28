using BitPantry.Tabs.Application;
using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;

namespace BitPantry.Tabs.Test.Playwright.Common
{
    [TestClass]
    public class CollectionTests : PageTest
    {
        private async Task<long> Init()
        {
            var userId = Fixture.Environment.CreateUser().GetAwaiter().GetResult();
            await Page.AuthenticateUser(userId);
            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/collection"));
            return userId;
        }

        [TestMethod]
        public async Task NoCardsNavigateTabs_AllElementsInPlace()
        {
            _ = await Init();

            var tabTestIds = new string[]
            {
                "collection.tabDaily",
                "collection.tabOdd",
                "collection.tabEven",
                "collection.tabWeekday",
                "collection.tabDate",
                "collection.tabQueue",
            };

            await Expect(Page.GetByTestId("collection.tabQueue")).ToHaveClassAsync(new Regex("active"));
            await Expect(Page.GetByTestId("collection.pnlNoCardsFound")).ToHaveCountAsync(1);

            foreach (var tabId in tabTestIds)
            {
                await Page.GetByTestId(tabId).ClickAsync();
                await Expect(Page.GetByTestId(tabId)).ToHaveClassAsync(new Regex("active"));
                await Expect(Page.GetByTestId("collection.pnlNoCardsFound")).ToHaveCountAsync(1);
            }
        }

        [TestMethod]
        public async Task DailyTab_AllElementsInPlace()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Tabs.Common.Tab.Daily);
            }

            await Page.GetByTestId("collection.tabDaily").ClickAsync();

            await Expect(Page.GetByTestId("card.maint.btnSendBackToQueue")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("card.maint.btnStartNow")).ToHaveCountAsync(0);
            await Expect(Page.GetByTestId("card.maint.btnDelete")).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading)).ToContainTextAsync("Romans 1:1");
            await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 Paul, a servant of Christ Jesus, called to be an apostle, set apart for the gospel of God,");


        }

        [TestMethod]
        public async Task QueueTabSingle_AllElementsInPlace()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Tabs.Common.Tab.Queue);
            }

            await Page.GetByTestId("collection.tabQueue").ClickAsync();

            await Expect(Page.GetByTestId("card.maint.btnSendBackToQueue")).ToHaveCountAsync(0);
            await Expect(Page.GetByTestId("card.maint.btnStartNow")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("card.maint.btnDelete")).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading)).ToContainTextAsync("Romans 1:1");
            await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 Paul, a servant of Christ Jesus, called to be an apostle, set apart for the gospel of God,");


        }

        [TestMethod]
        public async Task QueueTabMultiple_AllElementsInPlace()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp1 = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Tabs.Common.Tab.Queue);
                var resp2 = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:2", Tabs.Common.Tab.Queue);
                var resp3 = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:3", Tabs.Common.Tab.Queue);

                await Page.GetByTestId("collection.tabQueue").ClickAsync();

                await Expect(Page.GetByTestId("collection.cardList")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId($"collection.cardListItem_{resp1.Card.Id}")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId($"collection.cardListItem_{resp2.Card.Id}")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId($"collection.cardListItem_{resp3.Card.Id}")).ToBeVisibleAsync();

            }
        }

        [TestMethod]
        public async Task CardListOpenCard_CardMaintenanceOpened()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp1 = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Tabs.Common.Tab.Queue);
                var resp2 = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:2", Tabs.Common.Tab.Queue);
                var resp3 = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:3", Tabs.Common.Tab.Queue);

                await Page.GetByTestId("collection.tabQueue").ClickAsync();

                var items = await Page.GetByTestId("collection.cardList").GetByRole(AriaRole.Link).AllAsync();
                await items[0].ClickAsync();

                await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp1.Card.Id}"));
            }
        }

        [TestMethod]
        public async Task QueueTabReorderCardList_CardsReordered()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Tabs.Common.Tab.Queue);
                await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:2", Tabs.Common.Tab.Queue);
                await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:3", Tabs.Common.Tab.Queue);
                await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:4", Tabs.Common.Tab.Queue);

                var cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).OrderBy(c => c.NumberedCard.RowNumber).ToListAsync();

                var reorderAction = async (Card from, Card to) =>
                {
                    await Page.GetByTestId($"collection.cardListItemHandle_{from.Id}").DragToAsync(Page.GetByTestId($"collection.cardListItemHandle_{to.Id}"));
                    await Page.WaitForTimeoutAsync(500);

                    cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).OrderBy(c => c.NumberedCard.RowNumber).ToListAsync();

                    var reorderedCard = cards.Single(c => c.Id == from.Id);
                    reorderedCard.NumberedCard.RowNumber.Should().Be(to.NumberedCard.RowNumber);

                    var rowNum = 0L;
                    foreach (var card in cards)
                    {
                        rowNum.Should().BeLessThan(card.NumberedCard.RowNumber);
                        rowNum = card.NumberedCard.RowNumber;
                    }

                    await Page.ReloadAsync();
                };

                await Page.GetByTestId("collection.tabQueue").ClickAsync();


                await reorderAction(cards[0], cards[1]);
                await reorderAction(cards[0], cards[3]);
                await reorderAction(cards[0], cards[2]);
                await reorderAction(cards[3], cards[0]);
                await reorderAction(cards[1], cards[2]);
                await reorderAction(cards[1], cards[3]);
                await reorderAction(cards[2], cards[0]);

            }
        }
    }
}

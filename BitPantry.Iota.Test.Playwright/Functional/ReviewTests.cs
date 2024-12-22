using BitPantry.Iota.Application;
using BitPantry.Iota.Application.Service;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Playwright.Functional
{
    [TestClass]
    public class ReviewTests : PageTest
    {
        private async Task<long> Init()
        {
            var userId = Fixture.Environment.CreateUser().GetAwaiter().GetResult();
            await Page.AuthenticateUser(userId);
            return userId;
        }

        [TestMethod]
        public async Task NoCardsStartReview_NoCardsView()
        {
            _ = await Init();

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));

            await Expect(Page.GetByTestId("review.noCards.pnlNoCardsMessage")).ToHaveTextAsync("You don't have any cards to review");
            await Expect(Page.GetByTestId("review.noCards.btnCreateCards")).ToBeVisibleAsync();

            await Page.GetByTestId("review.noCards.btnCreateCards").ClickAsync();

            Page.Url.Should().BeIgnoreCase(Fixture.Environment.GetUrlBuilder().Build("/card/new"));
        }

        [TestMethod]
        public async Task DailyCardOnlyStartReview_ElementsInPlace()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var resp1 = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", Common.Tab.Daily);
            }

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));

            await Expect(Page.GetByTestId("review.tabDaily")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("review.tabs").GetByRole(AriaRole.Listitem)).ToHaveCountAsync(1);

            await Expect(Page.GetByTestId("review.pnlSummary")).ToContainTextAsync("Reviewing 1 card today");
            await Expect(Page.GetByTestId("review.pnlAddress")).ToContainTextAsync("Romans 1:1 (ESV)");
            await Expect(Page.GetByText("Click to Show passage")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("review.btnPromote")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("review.btnNext")).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task DailyCardOnlyNext_DoneView()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var resp1 = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", Common.Tab.Daily);
            }

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));
            await Page.GetByTestId("review.btnNext").ClickAsync();
            await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("Review/Done"));
        }

        [TestMethod]
        public async Task DoneViewRestart_Restarted()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var resp1 = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", Common.Tab.Daily);
            }

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));
            await Page.GetByTestId("review.btnNext").ClickAsync();
            await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("Review/Done"));
            await Page.GetByTestId("review.done.btnRestart").ClickAsync();
            await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("review/Daily/1"));
        }

        [TestMethod]
        public async Task DoneViewCreateCard_CreateCardView()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var resp1 = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", Common.Tab.Daily);
            }

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review")); 
            await Page.GetByTestId("review.btnNext").ClickAsync();
            await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("Review/Done"));
            await Page.GetByTestId("review.done.btnCreateCards").ClickAsync();
            await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("Card/New"));
        }

        [TestMethod]
        public async Task StartReviewOnlyOddEvenCard_OddEvenTabLoaded()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                _ = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", Common.Tab.Odd);
                _ = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:2", Common.Tab.Even);
            }

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));
            await Expect(Page.GetByTestId("review.tabOdd").Or(Page.GetByTestId("review.TabEven"))).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task DailyCardOnly_Promote()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var resp1 = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", Common.Tab.Daily);
            }

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));

            await Page.GetByTestId("review.btnPromote").ClickAsync();
            await Page.GetByTestId("review.btnConfirmPromoteCancel").ClickAsync();
            await Expect(Page.GetByTestId("review.diaConfirmPromote")).ToBeHiddenAsync();
            
            await Page.GetByTestId("review.btnPromote").ClickAsync();
            await Page.GetByTestId("review.btnConfirmPromote").ClickAsync();

            await Expect(Page.GetByTestId("review.tabOdd").Or(Page.GetByTestId("review.TabEven"))).ToBeVisibleAsync();

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));

            await Expect(Page.GetByTestId("review.tabOdd").Or(Page.GetByTestId("review.TabEven"))).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task NavigateMultiCardTab_Navigated()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var tab = Common.Tab.Saturday + DateTime.Now.Day;

                var resp1 = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", tab);
                var resp2 = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:2", tab);
                var resp3 = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:3", tab);

                //await Task.Delay(40000);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));
                await Expect(Page.GetByTestId($"review.tab{tab.ToString()}")).ToBeVisibleAsync();

                var idsAndCards = new Dictionary<string, CreateCardResponse>
                {
                    { "review.cardtab_1", resp1 },
                    { "review.cardtab_2", resp2 },
                    { "review.cardtab_3", resp3 }
                };

                foreach (var id in idsAndCards.Keys)
                    await Expect(Page.GetByTestId(id)).ToBeVisibleAsync();

                foreach (var id in idsAndCards.Keys)
                {
                    await Page.GetByTestId(id).ClickAsync();
                    await Expect(Page.GetByTestId(id)).ToHaveClassAsync(new Regex("active"));
                    await Expect(Page.GetByTestId("review.pnlAddress")).ToContainTextAsync(idsAndCards[id].Card.Address);
                }
            }
        }
    }
}

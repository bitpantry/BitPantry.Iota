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

namespace BitPantry.Iota.Test.Playwright.Common
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
        public async Task NoCardsStartReview_DailyReviewTab()
        {
            var userId = await Init();

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));
            await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("review/Daily/1"));
        }

        [TestMethod]
        public async Task DailyCardOnlyStartReview_ElementsInPlace()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var resp1 = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", Iota.Common.Tab.Daily);
            }

            await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));

            await Expect(Page.GetByTestId("review.tabDaily")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("review.tabs").GetByRole(AriaRole.Listitem)).ToHaveCountAsync(4);

            await Expect(Page.GetByTestId("review.pnlSummary")).ToContainTextAsync("Reviewing 1 card today");
            await Expect(Page.GetByTestId("review.pnlAddress")).ToContainTextAsync("Romans 1:1 (ESV)");

            await Expect(Page.GetByTestId("review.pnlPassageCollapsed")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("review.btnPromote")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("review.btnNext")).ToBeVisibleAsync();
        }




    }
}

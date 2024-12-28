using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Latency;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Playwright.Workflow
{
    internal static class CommonReviewLogic
    {
        public static async Task ProgressThroughReviewWithNextButton_AllElementsRight(IPage page, long userId, int day, Func<IPage, Tab, int, Task> evaluateTabElementsFunc)
        {
            var date = new DateTime(2024, 1, day, 12, 0, 0);

            await Fixture.Environment.CreateCards(userId, Fixture.BibleId);

            await page.SetUserTimezoneOverride("utc");
            await page.SetUserCurrentTimeUtcOverride(date);

            // daily

            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("review"));
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("review/Daily/1"));

            await evaluateTabElementsFunc(page, Tab.Daily, 1);

            await page.GetByTestId("review.btnNext").ClickAsync();

            // odd / even

            var oddEvenTab = date.Day % 2 == 0 ? Tab.Even : Tab.Odd;
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"review/{oddEvenTab}/1"));

            await evaluateTabElementsFunc(page, oddEvenTab, 1);

            await page.GetByTestId("review.btnNext").ClickAsync();

            // day of week

            var dowTab = (Tab)Enum.Parse(typeof(Tab), date.DayOfWeek.ToString());
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"review/{dowTab}/1"));

            await evaluateTabElementsFunc(page, dowTab, 1);

            await page.GetByTestId("review.btnNext").ClickAsync();

            // date

            var dateTab = Tab.Saturday + date.Day;
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"review/{dateTab}/1"));

            await evaluateTabElementsFunc(page, dateTab, 1);

            await page.GetByTestId("review.btnNext").ClickAsync();

            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"Review/Done"));
        }

        public static async Task ProgressThroughMultipleCardTabsWithNextButton_AllElementsRight(IPage page, IServiceScope scope, IWorkflowService wfSvc, long userId, Tab tab, Func<IPage, Tab, int, Task> evaluateTabElementsFunc)
        {
            await Fixture.Environment.CreateCards(userId, Fixture.BibleId);
            
            var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

            var userDateTime = tab.GetValidReviewDateTime();

            await page.SetUserTimezoneOverride("utc");
            await page.SetUserCurrentTimeUtcOverride(userDateTime);

            var cards = new[]
            {
                "rom 1:1",
                "rom 1:2",
                "rom 1:3"
            };

            foreach (var card in cards)
                await cardSvc.CreateCard(userId, Fixture.BibleId, card, tab);

            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));

            var path = await wfSvc.GetReviewPath(userId, userDateTime, CancellationToken.None);
            var pathQueue = new Queue<Tab>();
            foreach (var t in path.Path.Keys)
                pathQueue.Enqueue(t);

            do
            {
                var t = pathQueue.Dequeue();

                if (t == tab) break;

                await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"review/{t}/1"));
                await page.GetByTestId("review.btnNext").ClickAsync();
            } while (pathQueue.Count > 0);


            for (var i = 1; i <= cards.Count() + 1; i++)
            {
                await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"review/{tab}/{i}"));

                await evaluateTabElementsFunc(page, tab, cards.Count() + 1);

                await page.GetByTestId("review.btnNext").ClickAsync();
            }

            if (pathQueue.Count == 0)
                await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("Review/Done"));
            else
                await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"review/{pathQueue.Dequeue()}/1"));

        }

        public static async Task SingleCardReview_AllElementsRight(IPage page, IServiceScope scope, long userId, Tab tab, Func<IPage, Tab, int, Task> evaluateTabElementsFunc)
        {
            await page.SetUserTimezoneOverride("utc");
            await page.SetUserCurrentTimeUtcOverride(tab.GetValidReviewDateTime());

            var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

            await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", tab);

            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("review"));
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"review/{tab}/1"));

            await evaluateTabElementsFunc(page, tab, 1);

            await page.GetByTestId("review.btnNext").ClickAsync();

            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"Review/Done"));

        }

        public static async Task DoneViewRestart_Restarted(IPage page, IServiceScope scope, long userId)
        {
            var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
            _ = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", Iota.Common.Tab.Daily);

            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));
            await page.GetByTestId("review.btnNext").ClickAsync();
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("Review/Done"));
            await page.GetByTestId("review.done.btnRestart").ClickAsync();
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("review/Daily/1"));
        }

        public static async Task DoneViewCreateCard_CreateCardView(IPage page, IServiceScope scope, long userId)
        {
            var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
            _ = await cardSvc.CreateCard(userId, Fixture.BibleId, "romans 1:1", Iota.Common.Tab.Daily);

            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));
            await page.GetByTestId("review.btnNext").ClickAsync();
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("Review/Done"));
            await page.GetByTestId("review.done.btnCreateCards").ClickAsync();
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("Card/New"));
        }
    }
}

using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Playwright.Workflow.Basic
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

        [DataTestMethod]
        [DataRow(1)] // Odd, 1st
        [DataRow(2)] // Even, 2nd
        [DataRow(3)] // 3rd
        [DataRow(4)] // 4th
        [DataRow(5)] // 5th
        [DataRow(6)] // 6th
        [DataRow(7)] // Sunday, 7th
        [DataRow(8)] // Monday, 8th
        [DataRow(9)] // Tuesday, 9th
        [DataRow(10)] // Wednesday, 10th
        [DataRow(11)] // Thursday, 11th
        [DataRow(12)] // Friday, 12th
        [DataRow(13)] // Saturday, 13th
        [DataRow(14)] // 14th
        [DataRow(15)] // 15th
        [DataRow(16)] // 16th
        [DataRow(17)] // 17th
        [DataRow(18)] // 18th
        [DataRow(19)] // 19th
        [DataRow(20)] // 20th
        [DataRow(21)] // 21st
        [DataRow(22)] // 22nd
        [DataRow(23)] // 23rd
        [DataRow(24)] // 24th
        [DataRow(25)] // 25th
        [DataRow(26)] // 26th
        [DataRow(27)] // 27th
        [DataRow(28)] // 28th
        [DataRow(29)] // 29th
        [DataRow(30)] // 30th
        [DataRow(31)] // 31st
        public async Task ProgressThroughReviewUsingNextButton_AllElementsRight(int day)
        {
            var userId = await Init();
            using(var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonReviewLogic.ProgressThroughReviewWithNextButton_AllElementsRight(Page, scope, userId, day, EvaluateTabElements);
        }

        [DataTestMethod]
        [DataRow(Tab.Day1)]
        [DataRow(Tab.Day2)]
        [DataRow(Tab.Day3)]
        [DataRow(Tab.Day4)]
        [DataRow(Tab.Day5)]
        public async Task ProgressThroughMultipleCardTabsUsingNext_AllElementsRight(Tab tab)
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonReviewLogic.ProgressThroughMultipleCardTabsWithNextButton_AllElementsRight(Page, scope, scope.ServiceProvider.GetRequiredService<AdvancedWorkflowService>(), userId, tab, EvaluateTabElements);
        }

        [TestMethod]
        public async Task DoneViewRestart_Restarted()
        {
            long userId = await Init();
            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonReviewLogic.DoneViewRestart_Restarted(Page, scope, userId);
        }

        [TestMethod]
        public async Task DoneViewCreateCard_CreateCardView()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonReviewLogic.DoneViewCreateCard_CreateCardView(Page, scope, userId);
        }

        [TestMethod]
        public async Task GotoReviewView_CardMarkedAsReviewed()
        {
            long userId = await Init();
            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Tab.Daily);

                resp.Card.ReviewCount.Should().Be(0);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("review/daily/1"));

                var card = await cardSvc.GetCard(resp.Card.Id);

                card.ReviewCount.Should().Be(1);
            }

        }

        [DataTestMethod]
        [DataRow(1, 0)]
        [DataRow(0, 0)]
        [DataRow(0, 1)]
        [DataRow(0, 3)]
        public async Task ReviewNoDailyCard_AllElementsRight(int dailyCardCount, int queueCardCount)
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonReviewLogic.ReviewNoDailyCard_AllElementsRight(Page, scope, userId, dailyCardCount, queueCardCount, EvaluateTabElements);
        }

        [TestMethod]
        public async Task ReviewNoDailyCardClickAddCards_CardNewView()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 3:1", Tab.Odd);
                _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 3:2", Tab.Even);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));

                await EvaluateTabElements(Page, scope, userId, Tab.Daily, 0);

                await Page.GetByTestId("review.nodaily.btnCreateCards").ClickAsync();

                await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("Card/New"));
            }
        }

        [TestMethod]
        public async Task ReviewNoDailyCardClickPullQueue_QueueCardPulled()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 3:1", Tab.Odd);
                _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 3:2", Tab.Even);
                var queueCardResp = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 3:3", Tab.Queue);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build("/review"));

                await EvaluateTabElements(Page, scope, userId, Tab.Daily, 0);

                await Page.GetByTestId("review.nodaily.btnNextQueueCard").ClickAsync();

                await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("/review/Daily/1"));

                var queueCard = await cardSvc.GetCard(queueCardResp.Card.Id);
                queueCard.Tab.Should().Be(Tab.Daily);
            }
        }

        private async Task EvaluateTabElements(IPage page, IServiceScope scope, long userId, Tab tab, int expectedCardCount = 1)
        {
            var tabSvc = scope.ServiceProvider.GetRequiredService<TabsService>();
            var queueCardCount = await tabSvc.GetCardCountForTab(userId, Tab.Queue);

            if (expectedCardCount > 0)
            {
                if (tab == Tab.Daily)
                {
                    await Expect(page.GetByTestId("review.btnPromote")).ToBeVisibleAsync();
                    await Expect(page.GetByTestId("review.pnlPromoteBasicMsg")).ToHaveTextAsync("Promote this card to start on the next card from your queue.");
                    await EvaluatePromoteDialog(page);
                }
                else
                {
                    await Expect(page.GetByTestId("review.btnPromote")).ToHaveCountAsync(0);
                    await Expect(page.GetByTestId("review.pnlPromoteBasicMsg")).ToHaveCountAsync(0);
                }

                await Expect(page.GetByTestId("review.pnlPromoteAdvancedMsg")).ToHaveCountAsync(0);

                if (expectedCardCount > 1)
                {
                    await Expect(page.GetByTestId("review.subtabs")).ToBeVisibleAsync();

                    for (int x = 1; x <= expectedCardCount; x++)
                        await Expect(page.GetByTestId($"review.cardtab_{x}")).ToBeVisibleAsync();
                }
                else 
                {
                    await Expect(page.GetByTestId("review.subtabs")).ToHaveCountAsync(0);
                }

                await Expect(Page.GetByTestId("review.pnlReviewCountMsg")).ToHaveCountAsync(0);
                await Expect(Page.GetByTestId("review.btnGotIt")).ToHaveCountAsync(0);
            }
            else
            {
                await Expect(Page.GetByTestId("review.pnlNoCardsFound")).ToBeVisibleAsync();

                if (tab == Tab.Daily)
                {
                    if (queueCardCount > 0)
                    {
                        await Expect(Page.GetByTestId("review.nodaily.pnlNextQueueCardMsg")).ToBeVisibleAsync();
                        await Expect(Page.GetByTestId("review.nodaily.btnNextQueueCard")).ToBeVisibleAsync();
                        await Expect(Page.GetByTestId("review.nodaily.pnlNoQueueCardMsg")).ToHaveCountAsync(0);
                    }
                    else
                    {
                        await Expect(Page.GetByTestId("review.nodaily.pnlNextQueueCardMsg")).ToHaveCountAsync(0);
                        await Expect(Page.GetByTestId("review.nodaily.pnlNoQueueCardMsg")).ToBeVisibleAsync();
                        await Expect(Page.GetByTestId("review.nodaily.btnNextQueueCard")).ToHaveCountAsync(0);
                    }

                    await Expect(Page.GetByTestId("review.nodaily.btnCreateCards")).ToBeVisibleAsync();
                }
            }

            if (tab < Tab.Day1)
                await Expect(Page.GetByTestId("review.btnNext")).ToBeVisibleAsync();
        }

        private async Task EvaluatePromoteDialog(IPage page)
        {
            await Expect(page.GetByTestId("review.diaConfirmPromote")).ToBeHiddenAsync();

            await page.GetByTestId("review.btnPromote").ClickAsync();

            await Expect(page.GetByTestId("review.diaConfirmPromote")).ToBeVisibleAsync();
            await Expect(page.GetByTestId("review.btnConfirmPromote")).ToBeVisibleAsync();
            await Expect(page.GetByTestId("review.pnlPromoteConfirmDialogMsg")).ToContainTextAsync("will be moved to the next tab and the top card from your queue will be moved to the Daily tab.");

            await page.GetByTestId("review.btnConfirmPromoteCancel").ClickAsync();

            await Expect(page.GetByTestId("review.diaConfirmPromote")).ToBeHiddenAsync();
        }
    }
}

using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Data.Entity.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BitPantry.Iota.Test.Playwright.Workflow.Advanced
{
    [TestClass]
    public class ReviewTests : PageTest
    {
        private async Task<long> Init()
        {
            var userId = Fixture.Environment.CreateUser(WorkflowType.Advanced).GetAwaiter().GetResult();
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
        public async Task ProgressThroughReviewWithNextButton_AllElementsRight(int day)
        {
            var userId = await Init();
            await CommonReviewLogic.ProgressThroughReviewWithNextButton_AllElementsRight(Page, userId, day, EvaluateTabElements);
        }


        [DataTestMethod]
        [DataRow(Tab.Daily)]
        [DataRow(Tab.Odd)]
        [DataRow(Tab.Even)]
        [DataRow(Tab.Sunday)]
        [DataRow(Tab.Monday)]
        [DataRow(Tab.Tuesday)]
        [DataRow(Tab.Wednesday)]
        [DataRow(Tab.Thursday)]
        [DataRow(Tab.Friday)]
        [DataRow(Tab.Saturday)]
        [DataRow(Tab.Day1)]
        [DataRow(Tab.Day2)]
        [DataRow(Tab.Day3)]
        [DataRow(Tab.Day4)]
        [DataRow(Tab.Day5)]
        public async Task ProgressThroughMultipleCardTabsWithNextButton_AllElementsRight(Tab tab)
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonReviewLogic.ProgressThroughMultipleCardTabsWithNextButton_AllElementsRight(Page, scope, scope.ServiceProvider.GetRequiredService<AdvancedWorkflowService>(), userId, tab, EvaluateTabElements);
        }

        [DataTestMethod]
        [DataRow(Tab.Daily)]
        [DataRow(Tab.Odd)]
        [DataRow(Tab.Even)]
        [DataRow(Tab.Sunday)]
        [DataRow(Tab.Monday)]
        [DataRow(Tab.Tuesday)]
        [DataRow(Tab.Wednesday)]
        [DataRow(Tab.Thursday)]
        [DataRow(Tab.Friday)]
        [DataRow(Tab.Saturday)]
        [DataRow(Tab.Day1)]
        [DataRow(Tab.Day2)]
        [DataRow(Tab.Day3)]
        [DataRow(Tab.Day4)]
        [DataRow(Tab.Day5)]
        public async Task SingleCardReview(Tab tab)
        {
            long userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonReviewLogic.SingleCardReview_AllElementsRight(Page, scope, userId, tab, EvaluateTabElements);
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
            long userId = await Init();
            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
                await CommonReviewLogic.DoneViewCreateCard_CreateCardView(Page, scope, userId);
        }

        private async Task EvaluateTabElements(IPage page, Tab tab, int expectedCardCount)
        {
            if (expectedCardCount > 0)
            {
                if (tab < Tab.Day1)
                {
                    await Expect(Page.GetByTestId("review.btnPromote")).ToBeVisibleAsync();
                    await Expect(Page.GetByTestId("review.pnlPromoteAdvancedMsg")).ToHaveTextAsync("Promote this card to move it to the next tab.");
                    await EvaluatePromoteDialog();
                }
                else
                {
                    await Expect(Page.GetByTestId("review.btnPromote")).ToHaveCountAsync(0);
                    await Expect(Page.GetByTestId("review.pnlPromoteAdvancedMsg")).ToHaveCountAsync(0);
                }

                await Expect(Page.GetByTestId("review.pnlPromoteBasicMsg")).ToHaveCountAsync(0);

                if (expectedCardCount > 1)
                {
                    await Expect(Page.GetByTestId("review.subtabs")).ToBeVisibleAsync();

                    for (int x = 1; x <= expectedCardCount; x++)
                        await Expect(Page.GetByTestId($"review.cardtab_{x}")).ToBeVisibleAsync();
                }
                else
                {
                    await Expect(Page.GetByTestId("review.subtabs")).ToHaveCountAsync(0);
                }
            }
        }

        private async Task EvaluatePromoteDialog()
        {
            await Expect(Page.GetByTestId("review.diaConfirmPromote")).ToBeHiddenAsync();

            await Page.GetByTestId("review.btnPromote").ClickAsync();
            
            await Expect(Page.GetByTestId("review.diaConfirmPromote")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("review.btnConfirmPromote")).ToBeVisibleAsync();
            await Expect(Page.GetByTestId("review.pnlPromoteConfirmDialogMsg")).ToContainTextAsync("will be moved to the next tab.");
            
            await Page.GetByTestId("review.btnConfirmPromoteCancel").ClickAsync();

            await Expect(Page.GetByTestId("review.diaConfirmPromote")).ToBeHiddenAsync();
        }
    }
}

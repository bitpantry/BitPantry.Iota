using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Playwright.Workflow.Advanced
{
    [TestClass]
    public class CreateCardTests : PageTest
    {

        private async Task<long> Init()
        {
            var userId = Fixture.Environment.CreateUser(WorkflowType.Advanced).GetAwaiter().GetResult();
            await Page.AuthenticateUser(userId);
            await Page.GotoRelativeAsync("/card/new");
            return userId;
        }

        [TestMethod]
        public async Task CreateCardToDailyTab_CreatedInDailyTab()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
                cards.Should().BeEmpty();

                await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
                await Page.GetByTestId("card.new.btnGo").ClickAsync();
                await Page.GetByTestId("card.new.btnCreateDdl").ClickAsync();
                await Page.GetByTestId("card.new.btnAddToDaily").ClickAsync();
                await Expect(Page.GetByTestId("card.new.cardCreatedMsg")).ToContainTextAsync("Added Romans 1:1 to the Daily tab.");
                await Expect(Page.GetByTestId("card.new.viewAll")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.new.startReview")).ToBeVisibleAsync();

                cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
                cards.Should().HaveCount(1);
                cards[0].Address.Should().Be("Romans 1:1");
                cards[0].Tab.Should().Be(Tab.Daily);
            }
        }

        [TestMethod]
        public async Task CreateCardToQueueTab_CreatedInQueueTab()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
                cards.Should().BeEmpty();

                await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
                await Page.GetByTestId("card.new.btnGo").ClickAsync();
                await Page.GetByTestId("card.new.btnCreateDdl").ClickAsync();
                await Page.GetByTestId("card.new.btnAddToQueue").ClickAsync();
                await Expect(Page.GetByTestId("card.new.cardCreatedMsg")).ToContainTextAsync("Added Romans 1:1 to the Queue tab.");
                await Expect(Page.GetByTestId("card.new.viewAll")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.new.startReview")).ToBeVisibleAsync();

                cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
                cards.Should().HaveCount(1);
                cards[0].Address.Should().Be("Romans 1:1");
                cards[0].Tab.Should().Be(Tab.Queue);
            }
        }

        [TestMethod]
        public async Task CreateDuplicateCard_AlreadyCreated()
        {
            _ = await Init();

            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();

            await Page.GetByTestId("card.new.btnCreateDdl").ClickAsync();
            await Page.GetByTestId("card.new.btnAddToQueue").ClickAsync();

            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();

            await Expect(Page.GetByTestId("card.new.btnCreateDdl")).ToHaveCountAsync(0);
            await Expect(Page.GetByTestId("card.new.btnCreateDdlDisabled")).ToBeDisabledAsync();
            await Expect(Page.GetByTestId("card.new.cardAlreadyExistsMsg")).ToContainTextAsync("Card already exists");
        }

        [TestMethod]
        public async Task CreateCardAndViewAll_CollectionLoaded()
        {
            _ = await Init();

            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Page.GetByTestId("card.new.btnCreateDdl").ClickAsync();
            await Page.GetByTestId("card.new.btnAddToQueue").ClickAsync();
            await Page.GetByTestId("card.new.viewAll").ClickAsync();
            Page.Url.Should().BeIgnoreCase(Fixture.Environment.GetUrlBuilder().Build("collection"));
        }

        [TestMethod]
        public async Task CreateCardAndStartNow_CardStartedNow()
        {
            _ = await Init();

            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:3");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Page.GetByTestId("card.new.btnCreateDdl").ClickAsync();
            await Page.GetByTestId("card.new.btnAddToDaily").ClickAsync();
            await Page.GetByTestId("card.new.startReview").ClickAsync();
            Page.Url.Should().BeIgnoreCase(Fixture.Environment.GetUrlBuilder().Build("review/Daily/1"));

        }

        public override BrowserNewContextOptions ContextOptions()
            => Fixture.BrowserOptions.Default;
    }
}

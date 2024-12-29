using BitPantry.Iota.Application.Service;
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

namespace BitPantry.Iota.Test.Playwright.Workflow.Basic
{
    [TestClass]
    public class CreateCardTests : PageTest
    {

        private async Task<long> Init()
        {
            var userId = Fixture.Environment.CreateUser().GetAwaiter().GetResult();
            await Page.AuthenticateUser(userId);
            await Page.GotoRelativeAsync("/card/new");
            return userId;
        }

        [TestMethod]
        public async Task CreateFirstCard_AddedToDailyTab()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
                cards.Should().BeEmpty();

                await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
                await Page.GetByTestId("card.new.btnGo").ClickAsync();
                await Page.GetByTestId("card.new.createCard").ClickAsync();
                await Expect(Page.GetByTestId("card.new.cardCreatedMsg")).ToContainTextAsync("Added Romans 1:1 to the Daily tab.");
                await Expect(Page.GetByTestId("card.new.viewAll")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.new.startReview")).ToBeVisibleAsync();

                cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
                cards.Should().HaveCount(1);
                cards[0].Address.Should().Be("Romans 1:1");
                cards[0].Tab.Should().Be(Iota.Common.Tab.Daily);
            }
        }

        [TestMethod]
        public async Task CreateSecondCard_AddedToQueueTab()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
                cards.Should().BeEmpty();

                await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
                await Page.GetByTestId("card.new.btnGo").ClickAsync();
                await Page.GetByTestId("card.new.createCard").ClickAsync();

                cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
                cards.Should().HaveCount(1);
                cards[0].Address.Should().Be("Romans 1:1");
                cards[0].Tab.Should().Be(Iota.Common.Tab.Daily);

                await Page.GetByTestId("card.new.txtAddress").ClickAsync();
                await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:2");
                await Page.GetByTestId("card.new.btnGo").ClickAsync();
                await Expect(Page.GetByTestId("card.new.createCardDisabled")).ToHaveCountAsync(0);
                await Page.GetByTestId("card.new.createCard").ClickAsync();
                await Expect(Page.GetByTestId("card.new.cardCreatedMsg")).ToContainTextAsync("Added Romans 1:2 to the Queue tab.");

                cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
                cards.Should().HaveCount(2);
                cards[1].Address.Should().Be("Romans 1:2");
                cards[1].Tab.Should().Be(Iota.Common.Tab.Queue);
            }
        }

        [TestMethod]
        public async Task CreateDuplicateCard_AlreadyCreated()
        {
            _ = await Init();

            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Page.GetByTestId("card.new.createCard").ClickAsync();

            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Expect(Page.GetByTestId("card.new.createCard")).ToHaveCountAsync(0);
            await Expect(Page.GetByTestId("card.new.createCardDisabled")).ToBeDisabledAsync();
            await Expect(Page.GetByTestId("card.new.cardAlreadyExistsMsg")).ToContainTextAsync("Card already exists");
        }

        [TestMethod]
        public async Task CreateCardAndViewAll_CollectionLoaded()
        {
            _ = await Init();

            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:1");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Page.GetByTestId("card.new.createCard").ClickAsync();
            await Page.GetByTestId("card.new.viewAll").ClickAsync();
            Page.Url.Should().BeIgnoreCase(Fixture.Environment.GetUrlBuilder().Build("collection"));
        }

        [TestMethod]
        public async Task CreateCardAndStartNow_CardStartedNow()
        {
            _ = await Init();

            await Page.GetByTestId("card.new.txtAddress").FillAsync("rom 1:3");
            await Page.GetByTestId("card.new.btnGo").ClickAsync();
            await Page.GetByTestId("card.new.createCard").ClickAsync();
            await Page.GetByTestId("card.new.startReview").ClickAsync();
            Page.Url.Should().BeIgnoreCase(Fixture.Environment.GetUrlBuilder().Build("review/Daily/1"));

        }

        public override BrowserNewContextOptions ContextOptions()
            => Fixture.BrowserOptions.Default;
    }
}

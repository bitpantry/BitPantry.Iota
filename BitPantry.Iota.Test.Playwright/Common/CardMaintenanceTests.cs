using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Data.Entity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Playwright.Common
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

        [TestMethod]
        public async Task OpenDailyCard_AllElementsRight()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Iota.Common.Tab.Daily);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));
                await Expect(Page.GetByTestId("card.maint.tab")).ToContainTextAsync("Daily");
                await Expect(Page.GetByTestId("card.maint.btnClose")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.maint.btnSendBackToQueue")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.maint.btnDelete")).ToBeVisibleAsync();
                await Expect(Page.GetByRole(AriaRole.Heading)).ToContainTextAsync("Romans 1:1");
                await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 Paul, a servant of Christ Jesus, called to be an apostle, set apart for the gospel of God,");

                await Expect(Page.GetByTestId("card.maint.btnStartNow")).ToHaveCountAsync(0);
            }

        }

        [TestMethod]
        public async Task CloseDailyCard_BackToCollectionDaily()
        {

            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Iota.Common.Tab.Daily);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));
                await Page.GetByTestId("card.maint.btnClose").ClickAsync();

                Page.Url.Should().BeIgnoreCase(Fixture.Environment.GetUrlBuilder().Build("/collection/daily"));
            }

        }

        [TestMethod]
        public async Task SendDailyCardBackToEmptyQueue_SentBackToQueue()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Iota.Common.Tab.Daily);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));

                await Page.GetByTestId("card.maint.btnSendBackToQueue").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmSendToQueue")).ToBeVisibleAsync();
                await Page.GetByTestId("card.maint.btnConfirmSendToQueueCancel").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmSendToQueue")).ToBeHiddenAsync();

                await Page.GetByTestId("card.maint.btnSendBackToQueue").ClickAsync();
                await Page.GetByTestId("card.maint.btnConfirmSendToQueue").ClickAsync();
                await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("/collection/Queue"));

                var card = await dbCtx.Cards.Where(c => c.UserId == userId).SingleAsync();

                card.Tab.Should().Be(Iota.Common.Tab.Queue);
            }

        }

        [TestMethod]
        public async Task DeleteDailyCardEmptyQueue_CardDeleted()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Iota.Common.Tab.Daily);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));

                await Page.GetByTestId("card.maint.btnDelete").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmDelete")).ToBeVisibleAsync();
                await Page.GetByTestId("card.maint.btnConfirmDeleteCancel").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmDelete")).ToBeHiddenAsync();

                await Page.GetByTestId("card.maint.btnDelete").ClickAsync();
                await Page.GetByTestId("card.maint.btnConfirmDelete").ClickAsync();
                await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("/collection/Daily"));

                var card = dbCtx.Cards.Where(c => c.UserId == userId).SingleOrDefault();
                card.Should().BeNull();
            }

        }

        [TestMethod]
        public async Task OpenOddCard_AllElementsRight()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Iota.Common.Tab.Odd);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));
                await Expect(Page.GetByTestId("card.maint.tab")).ToContainTextAsync("Odd");
                await Expect(Page.GetByTestId("card.maint.btnClose")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.maint.btnSendBackToQueue")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.maint.btnDelete")).ToBeVisibleAsync();
                await Expect(Page.GetByRole(AriaRole.Heading)).ToContainTextAsync("Romans 1:1");
                await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 Paul, a servant of Christ Jesus, called to be an apostle, set apart for the gospel of God,");

                await Expect(Page.GetByTestId("card.maint.btnStartNow")).ToHaveCountAsync(0);
            }

        }

        [TestMethod]
        public async Task DeleteOddCard_CardDeleted()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Iota.Common.Tab.Odd);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));

                await Page.GetByTestId("card.maint.btnDelete").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmDelete")).ToBeVisibleAsync();
                await Page.GetByTestId("card.maint.btnConfirmDeleteCancel").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmDelete")).ToBeHiddenAsync();

                await Page.GetByTestId("card.maint.btnDelete").ClickAsync();
                await Page.GetByTestId("card.maint.btnConfirmDelete").ClickAsync();

                var card = dbCtx.Cards.Where(c => c.UserId == userId).SingleOrDefault();

                card.Should().BeNull();

                Page.Url.Should().BeIgnoreCase(Fixture.Environment.GetUrlBuilder().Build("/collection/odd"));
            }

        }

        [TestMethod]
        public async Task SendOddCardBackToQueue_CardSentToQueue()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Iota.Common.Tab.Odd);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));

                await Page.GetByTestId("card.maint.btnSendBackToQueue").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmSendToQueue")).ToBeVisibleAsync();
                await Page.GetByTestId("card.maint.btnConfirmSendToQueueCancel").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmSendToQueue")).ToBeHiddenAsync();

                await Page.GetByTestId("card.maint.btnSendBackToQueue").ClickAsync();
                await Page.GetByTestId("card.maint.btnConfirmSendToQueue").ClickAsync();

                await Page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build("/collection/Queue"));

                var card = await dbCtx.Cards.Where(c => c.UserId == userId).SingleAsync();
                card.Tab.Should().Be(Iota.Common.Tab.Queue);

            }

        }

        [TestMethod]
        public async Task OpenQueueCard_AllElementsRight()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Iota.Common.Tab.Queue);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));
                await Expect(Page.GetByTestId("card.maint.tab")).ToContainTextAsync("Queue");
                await Expect(Page.GetByTestId("card.maint.btnClose")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.maint.btnStartNow")).ToBeVisibleAsync();
                await Expect(Page.GetByTestId("card.maint.btnDelete")).ToBeVisibleAsync();
                await Expect(Page.GetByRole(AriaRole.Heading)).ToContainTextAsync("Romans 1:1");
                await Expect(Page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("1 Paul, a servant of Christ Jesus, called to be an apostle, set apart for the gospel of God,");

                await Expect(Page.GetByTestId("card.maint.btnSendBackToQueue")).ToHaveCountAsync(0);
            }

        }

        [TestMethod]
        public async Task StartQueueCardNowNoDaily_CardStarted()
        {
            var userId = await Init();

            using (var scope = Fixture.Environment.ServiceProvider.CreateScope())
            {
                var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", Iota.Common.Tab.Queue);

                await Page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));

                await Page.GetByTestId("card.maint.btnStartNow").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmStartNow")).ToBeVisibleAsync();
                await Page.GetByTestId("card.maint.btnConfirmStartNowCancel").ClickAsync();
                await Expect(Page.GetByTestId("card.maint.diaConfirmStartNow")).ToBeHiddenAsync();

                await Page.GetByTestId("card.maint.btnStartNow").ClickAsync();
                await Page.GetByTestId("card.maint.btnConfirmStartNow").ClickAsync();

                var card = await dbCtx.Cards.FindAsync(resp.Card.Id);
                card.Tab.Should().Be(Iota.Common.Tab.Daily);

                Page.Url.Should().BeIgnoreCase(Fixture.Environment.GetUrlBuilder().Build("/review/Daily/1"));
                await Expect(Page.GetByRole(AriaRole.Main)).ToContainTextAsync("Romans 1:1 (ESV)");
            }

        }


    }
}

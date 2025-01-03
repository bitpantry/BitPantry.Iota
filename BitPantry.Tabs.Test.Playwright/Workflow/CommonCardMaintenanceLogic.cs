using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Test.Playwright.Workflow
{
    public static class CommonCardMaintenanceLogic
    {
        public static async Task MaintenanceView_AllElementsRight(IPage page, IServiceScope scope, long userId, Tab tab, Func<IPage, Tab, string, string, Task> evalMaintViewFunc)
        {
            var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
            var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", tab);
            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));
            await evalMaintViewFunc(page, tab, "Romans 1:1", "1 Paul, a servant of Christ Jesus, called to be an apostle, set apart for the gospel of God,");
        }

        public static async Task CloseMaintenance_BackToCollectionTab(IPage page, IServiceScope scope, long userId, Tab tab)
        {
            var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
            var resp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", tab);
            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{resp.Card.Id}"));
            await page.GetByTestId("card.maint.btnClose").ClickAsync();
            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"/collection/{tab}"));
        }

        public static async Task SendCardToQueue_CardSentToQueue(IPage page, IServiceScope scope, long userId, Tab tab, int addtlCardsInTab, int cardsInQueue)
        {
            var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
            var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

            var cardResp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", tab);

            for (int i = 0; i < addtlCardsInTab; i++)
                _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 2:{i + 1}", tab);

            for (int i = 0; i < cardsInQueue; i++)
                _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 3:{i + 1}", Tab.Queue);

            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"/card/{cardResp.Card.Id}"));

            await page.GetByTestId("card.maint.btnSendBackToQueue").ClickAsync();
            await page.GetByTestId("card.maint.btnConfirmSendToQueue").ClickAsync();

            await page.WaitForURLAsync(Fixture.Environment.GetUrlBuilder().Build($"card/{cardResp.Card.Id}"));

            var card = await cardSvc.GetCard(cardResp.Card.Id);

            card.Tab.Should().Be(Tab.Queue);

            var queueCards = dbCtx.Cards.AsNoTracking().Where(c => c.Tab == Tab.Queue && c.UserId == userId).ToList();
            var tabCards = dbCtx.Cards.AsNoTracking().Where(c => c.Tab == tab && c.UserId == userId).ToList();

            queueCards.Count.Should().Be(cardsInQueue + 1);
            tabCards.Count.Should().Be(addtlCardsInTab);
        }

        public static async Task DeleteCard_CardDeleted(IPage page, IServiceScope scope, long userId, Tab tab, int addtlCardsInTab)
        {
            var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
            var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

            var cardResp = await cardSvc.CreateCard(userId, Fixture.BibleId, "rom 1:1", tab);

            for (var i = 1; i <= addtlCardsInTab; i++)
                _ = await cardSvc.CreateCard(userId, Fixture.BibleId, $"rom 2:{i}", tab);

            var cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == tab).ToListAsync();

            cards.Select(c => c.Id).Should().Contain(cardResp.Card.Id);
            cards.Should().HaveCount(addtlCardsInTab + 1);

            await page.GotoAsync(Fixture.Environment.GetUrlBuilder().Build($"card/{cardResp.Card.Id}"));

            await page.GetByTestId("card.maint.btnDelete").ClickAsync();
            await page.GetByTestId("card.maint.btnConfirmDelete").ClickAsync();

            await page.WaitForUrlAsyncIgnoreCase(Fixture.Environment.GetUrlBuilder().Build($"collection/{tab}"));

            cards = await dbCtx.Cards.AsNoTracking().Where(c => c.UserId == userId && c.Tab == tab).ToListAsync();

            cards.Select(c => c.Id).Should().NotContain(cardResp.Card.Id);
            cards.Should().HaveCount(addtlCardsInTab);
        }
    }
}

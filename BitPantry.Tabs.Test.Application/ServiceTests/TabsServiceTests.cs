using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Test.Application.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BitPantry.Tabs.Test.Application.ServiceTests;

[Collection("env")]
public class TabsServiceTests
{
    private readonly ApplicationEnvironment _env;
    private readonly long _bibleId;

    public TabsServiceTests(AppEnvironmentFixture fixture)
    {
        _env = fixture.Environment;
        _bibleId = fixture.BibleId;
    }

    [Fact]
    public async Task GetCardsForTab_SingleCard_LoadsVerses()
    {
        var userId = await _env.CreateUser();
        using var scope = _env.ServiceProvider.CreateScope();
        var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
        var tabsSvc = new TabsService(scope.ServiceProvider.GetRequiredService<EntityDataContext>());

        await cardSvc.CreateCard(userId, _bibleId, "gen 1:1", Tab.Daily, CancellationToken.None);

        var cards = await tabsSvc.GetCardsForTab(userId, Tab.Daily, CancellationToken.None);
        cards.Should().HaveCount(1);
        cards[0].Passage.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCardCountByTab_ReturnsCounts()
    {
        var userId = await _env.CreateUser();
        using var scope = _env.ServiceProvider.CreateScope();
        var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
        var tabsSvc = new TabsService(scope.ServiceProvider.GetRequiredService<EntityDataContext>());

        await cardSvc.CreateCard(userId, _bibleId, "gen 1:1", Tab.Daily, CancellationToken.None);
        await cardSvc.CreateCard(userId, _bibleId, "gen 1:2", Tab.Queue, CancellationToken.None);

        var counts = await tabsSvc.GetCardCountByTab(userId, CancellationToken.None);

        counts.Should().ContainKey(Tab.Daily);
        counts.Should().ContainKey(Tab.Queue);
        counts[Tab.Daily].Should().Be(1);
        counts[Tab.Queue].Should().Be(1);
    }
}

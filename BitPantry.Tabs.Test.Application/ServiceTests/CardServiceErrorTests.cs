using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Test.Application.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BitPantry.Tabs.Test.Application.ServiceTests;

[Collection("env")]
public class CardServiceErrorTests
{
    private readonly ApplicationEnvironment _env;
    private readonly long _bibleId;

    public CardServiceErrorTests(AppEnvironmentFixture fixture)
    {
        _env = fixture.Environment;
        _bibleId = fixture.BibleId;
    }

    [Fact]
    public async Task StartQueueCard_NotQueue_Throws()
    {
        var userId = await _env.CreateUser();
        using var scope = _env.ServiceProvider.CreateScope();
        var wfSvc = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
        var cardSvc = scope.ServiceProvider.GetRequiredService<CardService>();
        var card = await cardSvc.CreateCard(userId, _bibleId, "gen 1:1", Tab.Daily, CancellationToken.None);

        var act = async () => await wfSvc.StartQueueCard(card.Card.Id, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

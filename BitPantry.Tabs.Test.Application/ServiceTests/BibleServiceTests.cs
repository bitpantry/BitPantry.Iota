using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Application.Parsers.BibleData;
using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Test.Application.Fixtures;
using BitPantry.Tabs.Test;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BitPantry.Tabs.Test.Application.ServiceTests;

[Collection("env")]
public class BibleServiceTests
{
    private readonly ApplicationEnvironment _env;

    public BibleServiceTests(AppEnvironmentFixture fixture)
    {
        _env = fixture.Environment;
    }

    [Fact]
    public async Task InstallStream_BibleInstalled()
    {
        using var scope = _env.ServiceProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<BibleService>();

        var id = await svc.Install(new MemoryStream(TestResources.Translations.ESV), CancellationToken.None);

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetBiblePassage_ReturnsVerses()
    {
        using var scope = _env.ServiceProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<BibleService>();

        var bibleId = await svc.Install(new MemoryStream(TestResources.Translations.MSG), CancellationToken.None);
        var resp = await svc.GetBiblePassage(bibleId, "jn 3:16", CancellationToken.None);

        resp.Passage.Should().NotBeNull();
        resp.Passage.Verses.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task InstallBadXml_Throws()
    {
        using var scope = _env.ServiceProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<BibleService>();

        var act = async () => await svc.Install(new MemoryStream(TestResources.Translations.ESV_badClassification), CancellationToken.None);

        await act.Should().ThrowAsync<BibleDataParsingException>();
    }
}

using System.Xml.Linq;
using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Test.Application.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BitPantry.Tabs.Test.Application.ServiceTests;

[Collection("env")]
public class DataProtectionServiceTests
{
    private readonly ApplicationEnvironment _env;

    public DataProtectionServiceTests(AppEnvironmentFixture fixture)
    {
        _env = fixture.Environment;
    }

    [Fact]
    public async Task StoreAndReadKeys_KeysPersisted()
    {
        using var scope = _env.ServiceProvider.CreateScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
        var svc = new DataProtectionService(dbCtx);

        await svc.StoreDataProtectionKeys("<key>1</key>", DateTime.UtcNow);
        await svc.StoreDataProtectionKeys("<key>2</key>", DateTime.UtcNow);

        var keys = await svc.ReadDataProtectionKeys();

        keys.Should().HaveCount(2);
        keys.Select(k => k.ToString()).Should().Contain(new[]{"<key>1</key>", "<key>2</key>"});
    }

    [Fact]
    public async Task ReadKeys_NoKeys_EmptyCollection()
    {
        using var scope = _env.ServiceProvider.CreateScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
        var svc = new DataProtectionService(dbCtx);

        var keys = await svc.ReadDataProtectionKeys();

        keys.Should().BeEmpty();
    }
}

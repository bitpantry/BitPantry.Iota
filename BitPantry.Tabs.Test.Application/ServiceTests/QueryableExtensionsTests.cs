using BitPantry.Tabs.Infrastructure.Caching;
using System;
using System.Collections.Generic;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Test.Application.Fixtures;
using BitPantry.Tabs.Test.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace BitPantry.Tabs.Test.Application.ServiceTests;

[Collection("env")]
public class QueryableExtensionsTests
{
    private readonly ApplicationEnvironment _env;

    public QueryableExtensionsTests(AppEnvironmentFixture fixture)
    {
        _env = fixture.Environment;
    }

    [Fact]
    public async Task WithCaching_NotNoTracking_Throws()
    {
        using var scope = _env.ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

        db.Users.Add(new User { EmailAddress = "test@test.com" });
        await db.SaveChangesAsync();

        var query = db.Users;
        var svc = new CacheService(new DummyCache());
        Action act = () => query.WithCaching(svc);
        act.Should().Throw<InvalidOperationException>();
    }

    private class DummyCache : ICache
    {
        private readonly Dictionary<string, object> _data = new();
        public T Get<T>(string key) => (T)_data[key];
        public bool TryGetValue<T>(string key, out T outVal)
        {
            if(_data.TryGetValue(key, out var obj)) { outVal = (T)obj; return true; }
            outVal = default!; return false;
        }
        public void Set(string key, object obj, TimeSpan slidingExpiration) => _data[key] = obj;
    }
}

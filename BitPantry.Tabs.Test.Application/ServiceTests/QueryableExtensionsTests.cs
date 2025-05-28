using BitPantry.Tabs.Infrastructure.Caching;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace BitPantry.Tabs.Test.Application.ServiceTests;

public class QueryableExtensionsTests
{
    private class Dummy
    {
        public int Id { get; set; }
    }

    [Fact]
    public void WithCaching_NotNoTracking_Throws()
    {
        var options = new DbContextOptionsBuilder<DbContext>().UseInMemoryDatabase("dummy").Options;
        using var db = new DbContext(options);
        db.Set<Dummy>().Add(new Dummy());
        db.SaveChanges();

        var query = db.Set<Dummy>();
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

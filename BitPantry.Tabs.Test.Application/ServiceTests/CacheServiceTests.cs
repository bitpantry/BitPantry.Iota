using BitPantry.Tabs.Infrastructure.Caching;
using System.Collections.Generic;
using System;
using FluentAssertions;
using Xunit;

namespace BitPantry.Tabs.Test.Application.ServiceTests;

public class CacheServiceTests
{
    private class FakeCache : ICache
    {
        private readonly Dictionary<string, object> _dict = new();
        public T Get<T>(string key) => (T)_dict[key];
        public bool TryGetValue<T>(string key, out T outVal)
        {
            if (_dict.TryGetValue(key, out var obj))
            {
                outVal = (T)obj;
                return true;
            }
            outVal = default!;
            return false;
        }
        public void Set(string key, object obj, TimeSpan slidingExpiration)
        {
            _dict[key] = obj;
        }
    }

    [Fact]
    public async Task GetOrCreateAsync_CachesValue()
    {
        var cache = new FakeCache();
        var svc = new CacheService(cache);
        int callCount = 0;
        var result1 = await svc.GetOrCreateAsync("k", async () => { callCount++; return 5; });
        var result2 = await svc.GetOrCreateAsync("k", async () => { callCount++; return 6; });
        result1.Should().Be(5);
        result2.Should().Be(5);
        callCount.Should().Be(1);
    }
}

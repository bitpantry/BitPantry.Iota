using BitPantry.Iota.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitPantry.Iota.Infrastructure.Caching
{
    public class MemoryCache : ICache
    {
        private IMemoryCache _cache;

        public MemoryCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }

        public void Set(string key, object obj, TimeSpan slidingExpiration)
        {
            _cache.Set(key, obj, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });
        }
    }
}

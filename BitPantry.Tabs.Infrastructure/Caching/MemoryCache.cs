﻿using Microsoft.Extensions.Caching.Memory;

namespace BitPantry.Tabs.Infrastructure.Caching
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

        public bool TryGetValue<T>(string key, out T outVal)
        {
            if(_cache.TryGetValue(key, out object cachedData))
            {
                outVal = (T)cachedData;
                return true;
            }

            outVal = default;
            return false;
        }

        public void Set(string key, object obj, TimeSpan slidingExpiration)
        {
            _cache.Set(key, obj, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });
        }
       
    }
}

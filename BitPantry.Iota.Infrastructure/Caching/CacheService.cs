using System;

namespace BitPantry.Iota.Infrastructure.Caching;

public class CacheService
{
    private ICache _cache;

    public CacheService(ICache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> transientRetrieveFunction, TimeSpan? slidingExpiration = null)
    {
            if (!_cache.TryGetValue(key, out T cachedData))
            {
                // Data not in cache, retrieve it
                cachedData = await transientRetrieveFunction();

                // Save data in cache
                _cache.Set(key, cachedData, slidingExpiration ?? TimeSpan.FromMinutes(60));
            }

            return cachedData;
    }
}

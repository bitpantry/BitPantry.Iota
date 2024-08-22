using Microsoft.Extensions.Caching.Distributed;


namespace BitPantry.Iota.Infrastructure.Caching
{
    public class DistributedCache : ICache
    {
        private IDistributedCache _cache;

        public DistributedCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key)
        {
            var bytes = _cache.Get(key);
            if (bytes != null)
                return Serialization.Deserialize<T>(bytes);

            return default(T);
        }

        public bool TryGetValue<T>(string key, out T outVal)
        {
            var bytes = _cache.Get(key);
            if(bytes == null)
            {
                outVal = default;
                return false;
            }

            outVal = Serialization.Deserialize<T>(bytes);
            return true;
        }

        public void Set(string key, object obj, TimeSpan slidingExpiration)
        {
            _cache.Set(key, Serialization.Serialize(obj), new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration });
        }


    }
}

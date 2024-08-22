namespace BitPantry.Iota.Infrastructure.Caching
{
    public interface ICache
    {
        T Get<T>(string key);
        bool TryGetValue<T>(string key, out T outVal);
        void Set(string key, object obj, TimeSpan slidingExpiration);

    }
}

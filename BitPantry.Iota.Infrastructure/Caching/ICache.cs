using System;
using System.Collections.Generic;
using System.Text;

namespace BitPantry.Iota.Infrastructure.Caching
{
    public interface ICache
    {
        T Get<T>(string key);
        void Set(string key, object obj, TimeSpan slidingExpiration);
    }
}

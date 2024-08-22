using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BitPantry.Iota.Infrastructure.Caching;

public static class IQueryableExtensions
{
    public static CachedQueryable<T> WithCaching<T>(this IQueryable<T> query, CacheService cacheSvc, TimeSpan? slidingExpiration = null)
    {
        if(!query.IsNoTracking())
            throw new InvalidOperationException("The query must use AsNoTracking to use caching");

        return new(cacheSvc, query, slidingExpiration ?? TimeSpan.FromMinutes(60));
    }

    public static bool IsNoTracking<T>(this IQueryable<T> query)
        => query is IIncludableQueryable<T, object> || query is IAsyncEnumerable<T>;
}

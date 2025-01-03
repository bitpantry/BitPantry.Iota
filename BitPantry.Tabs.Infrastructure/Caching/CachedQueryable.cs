using System;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Tabs.Infrastructure.Caching;

public class CachedQueryable<T>
{
    private const string IQUERYABLE_KEY_PREFIX = "bp.tabs.query";

    private CacheService _cacheSvc;
    private IQueryable<T> _query;
    private readonly TimeSpan _slidingExpiration;
    private readonly string _key;

    internal CachedQueryable(CacheService cacheSvc, IQueryable<T> query, TimeSpan slidingExpiration)
    {
        _cacheSvc = cacheSvc;
        _query = query;
        _slidingExpiration = slidingExpiration;
        _key = $"{IQUERYABLE_KEY_PREFIX}_{query.ToQueryString().ComputeHash()}";
    }

    public async Task<List<T>> ToListAsync(CancellationToken cancellationToken)
        => await _cacheSvc.GetOrCreateAsync(_key, async () => await _query.ToListAsync(cancellationToken), _slidingExpiration);

    public async Task<T> SingleAsync(CancellationToken cancellationToken)
        => await _cacheSvc.GetOrCreateAsync(_key, async() => await _query.SingleAsync(cancellationToken), _slidingExpiration);
}

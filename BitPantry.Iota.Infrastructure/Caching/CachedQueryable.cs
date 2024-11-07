using System;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Infrastructure.Caching;

public class CachedQueryable<T>
{
    private const string IQUERYABLE_KEY_PREFIX = "bp.iota.query";

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

    public async Task<List<T>> ToListAsync()
        => await _cacheSvc.GetOrCreateAsync(_key, async () => await _query.ToListAsync(), _slidingExpiration);

    public async Task<T> SingleAsync()
        => await _cacheSvc.GetOrCreateAsync(_key, async() => await _query.SingleAsync(), _slidingExpiration);
}

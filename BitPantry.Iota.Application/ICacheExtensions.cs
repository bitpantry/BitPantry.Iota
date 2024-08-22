using System;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;

namespace BitPantry.Iota.Application;

public static class ICacheExtensions
{
    private const string KEY_BIBLES = "bitpantry.iota.application.bibles";

    public static List<Bible> GetBibles(this ICache cache)
        => cache.Get<List<Bible>>(KEY_BIBLES);

    public static void SetBibles(this ICache cache, List<Bible> bibles)
        => cache.Set(KEY_BIBLES, bibles, TimeSpan.FromMinutes(15));
    
}

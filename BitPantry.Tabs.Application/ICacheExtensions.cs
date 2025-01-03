using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Infrastructure.Caching;

namespace BitPantry.Tabs.Application;

public static class ICacheExtensions
{
    private const string KEY_BIBLES = "bitpantry.tabs.application.bibles";

    public static List<Bible> GetBibles(this ICache cache)
        => cache.Get<List<Bible>>(KEY_BIBLES);

    public static void SetBibles(this ICache cache, List<Bible> bibles)
        => cache.Set(KEY_BIBLES, bibles, TimeSpan.FromMinutes(15));
    
}

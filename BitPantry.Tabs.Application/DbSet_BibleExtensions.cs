using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Tabs.Application
{
    internal static class DbSet_BibleExtensions
    {
        public async static Task<Bible> SingleOrDefaultWithCacheAsync(this DbSet<Bible> dbSet, CacheService cache, long bibleId, CancellationToken cancellationToken)
        {
            var bibleList = await dbSet.AsNoTracking().WithCaching(cache).ToListAsync(cancellationToken);
            return bibleId == 0 ? bibleList.First() : bibleList.Single(b => b.Id == bibleId);
        }

    }
}

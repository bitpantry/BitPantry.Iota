using Azure.Core;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    internal static class DbSet_BibleExtensions
    {
        public async static Task<Bible> SingleOrDefaultWithCacheAsync(this DbSet<Bible> dbSet, CacheService cache, long bibleId)
        {
            var bibleList = await dbSet.AsNoTracking().WithCaching(cache).ToListAsync();
            return bibleId == 0 ? bibleList.First() : bibleList.Single(b => b.Id == bibleId);
        }

    }
}

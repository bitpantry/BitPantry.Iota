using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    internal static class DbSet_VerseExtensions
    {
        public async static Task<List<Verse>> ToListAsync(this DbSet<Verse> dbSet, long bibleId, int bookNumber, int chapterNumber, int fromVerse, int toVerse)
        {
            var query = dbSet.Where(
                v => v.Chapter.Number == chapterNumber && v.Chapter.Book.Number == bookNumber && v.Chapter.Book.Testament.Bible.Id == bibleId);

            if (toVerse == 0)
                query = query.Where(v => v.Number == fromVerse);
            else
                query = query.Where(v => v.Number >= fromVerse && v.Number <= toVerse);
            query = query.OrderBy(v => v.Number);

            return await query.ToListAsync();
        }

        public async static Task<List<Verse>> ToListWithCacheAsync(this DbSet<Verse> dbSet, CacheService cache, long bibleId, int bookNumber, int chapterNumber, int fromVerse, int toVerse)
        {
            var query = dbSet.AsNoTracking().Where(
                v => v.Chapter.Number == chapterNumber && v.Chapter.Book.Number == bookNumber && v.Chapter.Book.Testament.Bible.Id == bibleId);

            if (toVerse == 0)
                query = query.Where(v => v.Number == fromVerse);
            else
                query = query.Where(v => v.Number >= fromVerse && v.Number <= toVerse);
            query = query.OrderBy(v => v.Number);

            return await query.WithCaching(cache).ToListAsync();
        }
    }
}

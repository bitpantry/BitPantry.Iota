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
        public async static Task<List<Verse>> ToListAsync(
            this DbSet<Verse> dbSet,
            long bibleId,
            int bookNumber,
            int fromChapterNumber,
            int fromVerseNumber,
            int toChapterNumber,
            int toVerseNumber)
        {
            return await BuildGetPassageQuery(dbSet, bibleId, bookNumber, fromChapterNumber, fromVerseNumber, toChapterNumber, toVerseNumber)
                .ToListAsync();
        }

        public async static Task<List<Verse>> ToListWithCacheAsync(
            this DbSet<Verse> dbSet, 
            CacheService cache, 
            long bibleId, 
            int bookNumber, 
            int fromChapterNumber, 
            int fromVerseNumber, 
            int toChapterNumber, 
            int toVerseNumber)
        {
            return await BuildGetPassageQuery(dbSet, bibleId, bookNumber, fromChapterNumber, fromVerseNumber, toChapterNumber, toVerseNumber)
                .AsNoTracking()
                .WithCaching(cache)
                .ToListAsync();
        }

        private static IQueryable<Verse> BuildGetPassageQuery(
            this DbSet<Verse> dbSet, 
            long bibleId, 
            int bookNumber, 
            int fromChapterNumber, 
            int fromVerseNumber, 
            int toChapterNumber, 
            int toVerseNumber)
        {
            return dbSet.Where(v =>
                v.Chapter.Book.Testament.Bible.Id == bibleId &&
                v.Chapter.Book.Number == bookNumber &&
                (
                    (v.Chapter.Number == fromChapterNumber && v.Number >= fromVerseNumber && v.Chapter.Number == toChapterNumber && v.Number <= toVerseNumber) || // Single chapter and verse range
                    (v.Chapter.Number == fromChapterNumber && v.Number >= fromVerseNumber && v.Chapter.Number != toChapterNumber) || // Verses in the fromChapterNumber
                    (v.Chapter.Number == toChapterNumber && v.Number <= toVerseNumber && v.Chapter.Number != fromChapterNumber) || // Verses in the toChapterNumber
                    (v.Chapter.Number > fromChapterNumber && v.Chapter.Number < toChapterNumber) // Verses in chapters between fromChapterNumber and toChapterNumber
                ))
                .Include(v => v.Chapter)
                .OrderBy(v => v.Chapter.Number)
                .ThenBy(v => v.Number);
        }
    }
}

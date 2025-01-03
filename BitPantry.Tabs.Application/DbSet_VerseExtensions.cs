using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Tabs.Application
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
            int toVerseNumber,
            CancellationToken cancellationToken)
        {
            return await BuildGetPassageQuery(dbSet, bibleId, bookNumber, fromChapterNumber, fromVerseNumber, toChapterNumber, toVerseNumber)
                .AsNoTracking()
                .WithCaching(cache)
                .ToListAsync(cancellationToken);
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
                .ThenInclude(c => c.Book)
                .ThenInclude(b => b.Testament)
                .ThenInclude(t => t.Bible)
                .OrderBy(v => v.Id);
        }

        public static async Task<List<Verse>> ToListAsync(this DbSet<Verse> verses, long startVerseId, long endVerseId, CancellationToken cancellationToken, bool asNoTracking = true)
        {
            var query = asNoTracking ? verses.AsNoTracking() : verses;

            return await query
                .Include(v => v.Chapter)
                .ThenInclude(c => c.Book)
                .ThenInclude(b => b.Testament)
                .ThenInclude(t => t.Bible)
                .Where(v => v.Id >= startVerseId && v.Id <= endVerseId)
                .OrderBy(v => v.Id)
                .ToListAsync(cancellationToken);
        }
    }
}

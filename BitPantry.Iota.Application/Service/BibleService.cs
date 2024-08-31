using Azure.Core;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public class BibleService
    {
        private EntityDataContext _dbCtx;
        private CacheService _cacheSvc;

        public BibleService(EntityDataContext dbCtx, CacheService cacheSvc)
        {
            _dbCtx = dbCtx;
            _cacheSvc = cacheSvc;
        }

        public async Task<GetPassageResult> GetPassage(long bibleId, string address, bool asNoTracking = false)
        {
            if (string.IsNullOrEmpty(address))
                return new GetPassageResult { Code = GetPassageResultCode.CannotParseAddress };

            // parse the raw address text

            var parser = new BiblePassageAddress(address);
            if (!parser.IsValid)
                return new GetPassageResult { Code = GetPassageResultCode.CannotParseAddress };

            // get the target bible translation

            var bible = await _dbCtx.Bibles.SingleOrDefaultWithCacheAsync(_cacheSvc, bibleId);

            // resolve the given book name to an actual book

            var bookName = BookNameDictionary.Get(bible.Classification, parser.Book);

            if (bookName.Key == 0)
                return new GetPassageResult { Code = GetPassageResultCode.BookNotFound };

            // read verses

            var verses = asNoTracking
                    ? await _dbCtx.Verses.ToListWithCacheAsync(_cacheSvc, bible.Id, bookName.Key, parser.FromChapterNumber, parser.FromVerseNumber, parser.ToChapterNumber, parser.ToVerseNumber)
                    : await _dbCtx.Verses.ToListAsync(bible.Id, bookName.Key, parser.FromChapterNumber, parser.FromVerseNumber, parser.ToChapterNumber, parser.ToVerseNumber);

            // return result

             return new GetPassageResult
            {
                Code = GetPassageResultCode.Ok,
                BibleId = bible.Id,
                BookName = bookName.Value.Name,
                FromChapterNumber = parser.FromChapterNumber,
                FromVerseNumber = parser.FromVerseNumber,
                ToChapterNumber = verses.Last().Chapter.Number,
                ToVerseNumber = verses.Last().Number,
                Verses = verses
            };
        }
    }
}

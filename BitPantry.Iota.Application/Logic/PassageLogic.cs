using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;

namespace BitPantry.Iota.Application.Logic
{
    public class PassageLogic
    {
        public async Task<GetPassageResponse> GetPassageQuery(EntityDataContext dbCtx, CacheService cacheSvc, long bibleId, string addressString, CancellationToken cancellationToken)
        {
            // parse passage address

            var bible = await dbCtx.Bibles.SingleOrDefaultWithCacheAsync(cacheSvc, bibleId, cancellationToken);

            try
            {
                var parser = new PassageAddressParser(bible, addressString);

                // read verses

                var verses = await dbCtx.Verses.ToListWithCacheAsync(
                    cacheSvc,
                    bible.Id,
                    parser.BookNumber,
                    parser.FromChapterNumber,
                    parser.FromVerseNumber,
                    parser.ToChapterNumber,
                    parser.ToVerseNumber,
                    cancellationToken);

                // return result

                return new GetPassageResponse(verses.ToPassageDto(), null);
            }
            catch(PassageAddressParsingException ex)
            {
                // return errored response

                return new GetPassageResponse(null, ex);
            }
        }
    }


}

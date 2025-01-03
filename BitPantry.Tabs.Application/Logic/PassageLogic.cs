using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Application.Parsers;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Infrastructure.Caching;

namespace BitPantry.Tabs.Application.Logic
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

                if(verses.Count == 0)
                    throw new PassageAddressParsingException(bible.Id, addressString, PassageAddressParsingExceptionCode.InvalidAddress, "The address is invalid - no verses found");

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

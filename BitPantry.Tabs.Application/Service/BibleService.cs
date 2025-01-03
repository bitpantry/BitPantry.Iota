using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Application.Logic;
using BitPantry.Tabs.Application.Parsers.BibleData;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Tabs.Application.Service
{
    public class BibleService
    {
        private readonly EntityDataContext _dbCtx;
        private readonly CacheService _cacheSvc;
        private readonly PassageLogic _passageLogic;

        public BibleService(EntityDataContext dbCtx, CacheService cacheSvc, PassageLogic passageLogic)
        {
            _dbCtx = dbCtx;
            _cacheSvc = cacheSvc;
            _passageLogic = passageLogic;
        }

        public async Task<long> Install(Stream stream, CancellationToken cancellationToken)
            => await Install(new DefaultXmlBibleDataParser().Parse(stream), cancellationToken);

        public async Task<long> Install(string bibleDataFilePath, CancellationToken cancellationToken)
            => await Install(new DefaultXmlBibleDataParser().Parse(bibleDataFilePath), cancellationToken);

        private async Task<long> Install(Bible bible, CancellationToken cancellationToken)
        {
            _dbCtx.Bibles.Add(bible);
            await _dbCtx.SaveChangesAsync(cancellationToken);

            return bible.Id;
        }

        public async Task<GetPassageResponse> GetBiblePassage(long bibleId, string addressString, CancellationToken cancellationToken)
        {
            return await _passageLogic.GetPassageQuery(_dbCtx, _cacheSvc, bibleId, addressString, cancellationToken);
        }

        public async Task<List<BibleDto>> GetBibleTranslations(CancellationToken cancellationToken)
        {
            var bibles = await _dbCtx.Bibles.AsNoTracking().WithCaching(_cacheSvc).ToListAsync(cancellationToken);
            return bibles.Select(b => new BibleDto(b.Id, b.TranslationShortName, b.TranslationLongName)).ToList();
        }

    }

    public record GetBiblePassageQueryResponse(
        string ParsingError = null,
        PassageDto Passage = null);
}

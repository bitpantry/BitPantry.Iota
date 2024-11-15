using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Application.Parsers.BibleData;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application.Service
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

        public async Task<long> Install(string bibleDataFilePath, CancellationToken cancellationToken)
        {
            var newBible = new DefaultXmlBibleDataParser().Parse(bibleDataFilePath);
            _dbCtx.Bibles.Add(newBible);
            await _dbCtx.SaveChangesAsync(cancellationToken);

            return newBible.Id;
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

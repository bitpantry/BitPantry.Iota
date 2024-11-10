using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Bible.Query
{
    public class GetBibleTranslationsQueryHandler : IRequestHandler<GetBibleTranslationsQuery, List<BibleDto>>
    {
        private const string KEY_TRANSLATIONS_CACHE = "bitpantry.iota.cache.bibletranslations";

        private readonly EntityDataContext _dbCtx;
        private readonly CacheService _cacheSvc;

        public GetBibleTranslationsQueryHandler(EntityDataContext dbCtx, CacheService cacheSvc)
        {
            _dbCtx = dbCtx;
            _cacheSvc = cacheSvc;
        }

        public async Task<List<BibleDto>> Handle(GetBibleTranslationsQuery request, CancellationToken cancellationToken)
        {
            var bibles = await _dbCtx.Bibles.AsNoTracking().WithCaching(_cacheSvc).ToListAsync();
            return bibles.Select(b => new BibleDto(b.Id, b.TranslationShortName, b.TranslationLongName)).ToList();
        }
    }

    public record GetBibleTranslationsQuery : IRequest<List<BibleDto>> { }

}

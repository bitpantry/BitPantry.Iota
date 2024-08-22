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
    public class GetBibleTranslationsQueryHandler : IRequestHandler<GetBibleTranslationsQuery, GetBibleTranslationsQueryResponse>
    {
        private const string KEY_TRANSLATIONS_CACHE = "bitpantry.iota.cache.bibletranslations";

        private readonly EntityDataContext _dbCtx;
        private readonly CacheService _cacheSvc;

        public GetBibleTranslationsQueryHandler(EntityDataContext dbCtx, CacheService cacheSvc)
        {
            _dbCtx = dbCtx;
            _cacheSvc = cacheSvc;
        }

        public async Task<GetBibleTranslationsQueryResponse> Handle(GetBibleTranslationsQuery request, CancellationToken cancellationToken)
        {
            var bibles = await _dbCtx.Bibles.AsNoTracking().WithCaching(_cacheSvc).ToListAsync();
            return new GetBibleTranslationsQueryResponse(
                bibles.Select(b => new GetBibleTranslationsQueryResponseItem(b.Id, b.TranslationShortName, b.TranslationLongName)).ToList());
        }
    }

    public record GetBibleTranslationsQuery : IRequest<GetBibleTranslationsQueryResponse> { }

    public record GetBibleTranslationsQueryResponse(List<GetBibleTranslationsQueryResponseItem> Translations) { }

    public record GetBibleTranslationsQueryResponseItem(long Id, string ShortName, string LongName) { }

}

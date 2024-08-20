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
        private readonly ICache _cache;

        public GetBibleTranslationsQueryHandler(EntityDataContext dbCtx, ICache cache)
        {
            _dbCtx = dbCtx;
            _cache = cache;
        }

        public async Task<GetBibleTranslationsQueryResponse> Handle(GetBibleTranslationsQuery request, CancellationToken cancellationToken)
        {
            var items = _cache.Get<List<GetBibleTranslationsQueryResponseItem>>(KEY_TRANSLATIONS_CACHE);

            if(items == null)
            {
                items = await _dbCtx.Bibles
                .AsNoTracking()
                .Select(b => new GetBibleTranslationsQueryResponseItem(b.Id, b.TranslationShortName, b.TranslationLongName))
                .ToListAsync();

                _cache.Set(KEY_TRANSLATIONS_CACHE, items, TimeSpan.FromMinutes(15));
            }

            return new GetBibleTranslationsQueryResponse(items);
        }
    }

    public record GetBibleTranslationsQuery : IRequest<GetBibleTranslationsQueryResponse> { }

    public record GetBibleTranslationsQueryResponse(List<GetBibleTranslationsQueryResponseItem> Translations) { }

    public record GetBibleTranslationsQueryResponseItem(long Id, string ShortName, string LongName) { }

}

using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Card.Query
{
    public class GetCardQueryHandler : IRequestHandler<GetCardQuery, GetCardQueryResponse>
    {
        private EntityDataContext _dbCtx;

        public GetCardQueryHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<GetCardQueryResponse> Handle(GetCardQuery request, CancellationToken cancellationToken)
        {
            var card = await _dbCtx.Cards
                .AsNoTracking()
                .Include(c => c.Verses)
                .ThenInclude(v => v.Chapter)
                .ThenInclude(c => c.Book)
                .ThenInclude(b => b.Testament)
                .ThenInclude(t => t.Bible)
                .Where(c => c.Id == request.Id).FirstOrDefaultAsync();

            return new GetCardQueryResponse
            (
                card.Id,
                card.AddedOn,
                card.LastMovedOn,
                card.Divider,
                card.Verses.ToDictionary(v => v.Number, v => v.Text)
            );
        }
    }

    public record GetCardQuery(long Id) : IRequest<GetCardQueryResponse> { }

    public record GetCardQueryResponse(
        long Id, 
        DateTime AddedOn,
        DateTime LastMovedOn,
        Divider Divider,
        Dictionary<int, string> Verses);

}

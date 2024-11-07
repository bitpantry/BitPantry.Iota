using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
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
    public class GetCardQueryHandler : IRequestHandler<GetCardQuery, CardDto>
    {
        private EntityDataContext _dbCtx;
        private CardLogic _cardLgc;

        public GetCardQueryHandler(EntityDataContext dbCtx, CardLogic cardLgc)
        {
            _dbCtx = dbCtx;
            _cardLgc = cardLgc;
        }

        public async Task<CardDto> Handle(GetCardQuery request, CancellationToken cancellationToken)
        {
            // get card

            var query = _dbCtx.Cards
                .AsNoTracking();

            if (request.Id > 0)
                query = query.Where(c => c.Id == request.Id);
            else
                query = query.Where(c => c.UserId == request.UserId && c.Tab == request.Tab && c.Order == request.Order);

            var card = await query.SingleOrDefaultAsync();

            if (card == null)
                return null;

            // get verses

            var verses = request.IncludePassage ? await _dbCtx.Verses.ToListAsync(card.StartVerseId, card.EndVerseId, cancellationToken) : null;

            // return card dto

            return card.ToDto(verses);
        }
    }

    public class GetCardQuery : IRequest<CardDto> 
    {
        public long Id { get; }
        public long UserId { get; }
        public Tab Tab { get; }
        public int Order { get; }

        public GetCardQuery(long id) => Id = id;

        public bool IncludePassage { get; set; } = true;

        public GetCardQuery(long userId, Tab tab, int order)
        {
            UserId = userId;
            Tab = tab;
            Order = order;
        }

    }
}

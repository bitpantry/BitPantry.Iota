using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Data.Entity.Migrations;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Tabs.Query
{
    public class GetCardsForTabQueryHandler : IRequestHandler<GetCardsForTabQuery, List<CardDto>>
    {
        private EntityDataContext _dbCtx;
        private CardLogic _cardLgc;

        public GetCardsForTabQueryHandler(EntityDataContext dbCtx, CardLogic cardLgc) 
        {
            _dbCtx = dbCtx;
            _cardLgc = cardLgc;
        }

        public async Task<List<CardDto>> Handle(GetCardsForTabQuery request, CancellationToken cancellationToken)
        {
            var cards = await _dbCtx.Cards
                .AsNoTracking()
                .Where(c => c.UserId == request.UserId && c.Tab == request.Tab)
                .OrderBy(c => c.Order)
                .ToListAsync(cancellationToken);

            if (cards.Count == 1)
                return [.. (await Task.WhenAll(cards.Select(c => c.ToDtoLoadVerses(_dbCtx, cancellationToken))))];
            else
                return cards.Select(c => c.ToDto()).ToList();
        }
    }


    public record GetCardsForTabQuery(long UserId, Tab Tab) : IRequest<List<CardDto>> { }
}

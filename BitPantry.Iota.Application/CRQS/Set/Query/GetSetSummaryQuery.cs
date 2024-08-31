using BitPantry.Iota.Application;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Caching;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Set.Query
{
	public class GetSetSummaryHandler : IRequestHandler<GetSetSummaryQuery, GetSetSummaryResponse>
    {
        private EntityDataContext _dbCtx;

        public GetSetSummaryHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<GetSetSummaryResponse> Handle(GetSetSummaryQuery request, CancellationToken cancellationToken)
        {
            var cards = await _dbCtx.Cards.AsNoTracking()
                .Include(c => c.Verses)
                    .ThenInclude(v => v.Chapter)
                    .ThenInclude(c => c.Book)
                    .ThenInclude(b => b.Testament)
                    .ThenInclude(t => t.Bible)
                .Where(c => c.UserId == request.UserId)
                .GroupBy(c => c.Divider)
                .ToListAsync();

            return new GetSetSummaryResponse(
                    cards.SingleOrDefault(c => c.Key == Divider.Queue)?.Count() ?? 0,
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Daily)),
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Odd)),
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Even)),
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Sunday)),
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Monday)),
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Tuesday)),
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Wednesday)),
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Thursday)),
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Friday)),
                    GetCardSummaryCardInfo(cards.FirstOrDefault(c => c.Key == Divider.Saturday)),
                    cards.SingleOrDefault(c => c.Key >= Divider.Day1 && c.Key <= Divider.Day31)?.Count() ?? 0
                );

        }

        private CardSummaryInfo GetCardSummaryCardInfo(IGrouping<Divider, Data.Entity.Card> grouping)
        {
            if (grouping == null)
                return null;

            return grouping.FirstOrDefault().ToCardSummaryInfo();
        }
    }

    public record GetSetSummaryQuery(long UserId) : IRequest<GetSetSummaryResponse> { }

    public record GetSetSummaryResponse(
        int QueueCardCount,
        CardSummaryInfo Daily,
        CardSummaryInfo Odd,
        CardSummaryInfo Even,
        CardSummaryInfo Sunday,
        CardSummaryInfo Monday,
        CardSummaryInfo Tuesday,
        CardSummaryInfo Wednesday,
        CardSummaryInfo Thursday,
        CardSummaryInfo Friday,
        CardSummaryInfo Saturday,
        int DaysOfTheMonthCardCount
        )
    { }
}

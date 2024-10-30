using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Set.Query
{
    public class GetDaysOfTheMonthSetQueryHandler : IRequestHandler<GetDaysOfTheMonthSetQuery, Dictionary<int, int>>
    {
        private EntityDataContext _dbCtx;

        public GetDaysOfTheMonthSetQueryHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<Dictionary<int, int>> Handle(GetDaysOfTheMonthSetQuery request, CancellationToken cancellationToken)
        {
            var cards = await _dbCtx.Cards.AsNoTracking()
                .Where(c => c.UserId == request.UserId
                    && c.Tab >= Tab.Day1 && c.Tab <= Tab.Day31)
                .GroupBy(c => c.Tab)
                .ToListAsync();

            var dayIndex = 0;
            var data = new Dictionary<int, int>();
            for (var i = Tab.Day1; i <= Tab.Day31; i++)
            {
                dayIndex++;
                var grouping = cards.SingleOrDefault(c => c.Key == i);

                if (grouping != null)
                    data.Add(dayIndex, grouping.Count());
                else
                    data.Add(dayIndex, 0);
            }

            return data;
        }
    }

    public record GetDaysOfTheMonthSetQuery(long UserId) : IRequest<Dictionary<int, int>> { }
}

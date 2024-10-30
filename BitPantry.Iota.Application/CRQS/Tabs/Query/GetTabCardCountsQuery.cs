using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Tabs.Query
{
    public class GetTabCardCountsQueryHandler : IRequestHandler<GetTabCardCountsQuery, Dictionary<Tab, int>>
    {
        private EntityDataContext _dbCtx;

        public GetTabCardCountsQueryHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<Dictionary<Tab, int>> Handle(GetTabCardCountsQuery request, CancellationToken cancellationToken)
        {
            return await _dbCtx.Cards
                .GroupBy(card => card.Tab)
                .ToDictionaryAsync(group => group.Key, group => group.Count());
        }
    }

    public record GetTabCardCountsQuery(long UserId) : IRequest<Dictionary<Tab, int>> { }
}

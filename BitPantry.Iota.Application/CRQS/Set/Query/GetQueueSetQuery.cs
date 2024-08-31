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
	public class GetQueueSetQueryHandler : IRequestHandler<GetQueueSetQuery, List<CardSummaryInfo>>
	{
		private EntityDataContext _dbCtx;

		public GetQueueSetQueryHandler(EntityDataContext dbCtx)
		{
			_dbCtx = dbCtx;
		}

		public async Task<List<CardSummaryInfo>> Handle(GetQueueSetQuery request, CancellationToken cancellationToken)
		{
			return await _dbCtx.Cards.AsNoTracking()
				.Include(c => c.Verses)
					.ThenInclude(v => v.Chapter)
					.ThenInclude(c => c.Book)
					.ThenInclude(b => b.Testament)
					.ThenInclude(t => t.Bible)
				.Where(c => c.UserId == request.UserId && c.Divider == Divider.Queue)
				.OrderBy(c => c.Order)
				.Select(c => c.ToCardSummaryInfo())				
				.ToListAsync();
		}
	}

	public record GetQueueSetQuery(long UserId) : IRequest<List<CardSummaryInfo>>;

}

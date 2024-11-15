using BitPantry.Iota.Application.CRQS.Review.Query;
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
    public class GetHomeCardInfoQueryHandler : IRequestHandler<GetHomeCardInfoQuery, GetHomeCardInfoQueryResponse>
    {
        private EntityDataContext _dbCtx;
        private IMediator _med;

        public GetHomeCardInfoQueryHandler(EntityDataContext dbCtx, IMediator med)
        {
            _dbCtx = dbCtx;
            _med = med;
        }
        public async Task<GetHomeCardInfoQueryResponse> Handle(GetHomeCardInfoQuery request, CancellationToken cancellationToken)
        {
            var reviewPath = await _med.Send(new GetReviewPathQuery(request.UserId, request.UserLocalTime));

            return new GetHomeCardInfoQueryResponse(
                    await _dbCtx.Cards.CountAsync(c => c.UserId == request.UserId, cancellationToken),
                    reviewPath.Sum(p => p.Value)
                );
        }
    }

    public record GetHomeCardInfoQuery(long UserId, DateTime UserLocalTime) : IRequest<GetHomeCardInfoQueryResponse> { }

    public record GetHomeCardInfoQueryResponse(int TotalCardCount, int CardsToReviewTodayCount) { }
 
}

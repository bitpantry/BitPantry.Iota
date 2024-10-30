using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Card.Query
{

    public class GetNextCardForReviewQueryHandler : IRequestHandler<GetNextCardForReviewQuery, GetNextCardForReviewQueryResponse>
    {
        private EntityDataContext _dbCtx;
        private ReviewService _revSvc;

        public GetNextCardForReviewQueryHandler(EntityDataContext dbCtx, ReviewService revSvc)
        {
            _dbCtx = dbCtx;
            _revSvc = revSvc;
        }

        public async Task<GetNextCardForReviewQueryResponse> Handle(GetNextCardForReviewQuery request, CancellationToken cancellationToken)
        {
            var resp = await _revSvc.GetNextCardForReview(_dbCtx, request.UserId, request.CurrentTab, request.CurrentCardOrder);

            if(resp == null)
                return null;

            _ = await _dbCtx.SaveChangesAsync();

            return new GetNextCardForReviewQueryResponse
            (
                resp.Id,
                resp.AddedOn,
                resp.LastMovedOn,
                resp.LastReviewedOn,
                resp.Tab,
                resp.Order);
        }
    }

    public record GetNextCardForReviewQuery(long UserId, Tab? CurrentTab = null, int CurrentCardOrder = 1) : IRequest<GetNextCardForReviewQueryResponse> { }

    public record GetNextCardForReviewQueryResponse(
        long Id,
        DateTime AddedOn,
        DateTime LastMovedOn,
        DateTime LastReviewedOn,
        Tab Tab,
        int Order)
    { }
}

using BitPantry.Iota.Application.CRQS.Identity.Queries;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.ReviewSession.Command
{
    public class GetReviewSessionCommandHandler : IRequestHandler<GetReviewSessionCommand, GetReviewSessionCommandHandlerResponse>
    {
        private EntityDataContext _dbCtx;
        private ReviewService _reviewSvc;

        public GetReviewSessionCommandHandler(EntityDataContext dbCtx, ReviewService reviewSvc)
        {
            _dbCtx = dbCtx;
            _reviewSvc = reviewSvc;
        }

        public async Task<GetReviewSessionCommandHandlerResponse> Handle(GetReviewSessionCommand request, CancellationToken cancellationToken)
        {
            var sessionResp = await _reviewSvc.GetReviewSession(_dbCtx, request.UserId, request.StartNew);
            _dbCtx.SaveChanges();

            return new GetReviewSessionCommandHandlerResponse(
                sessionResp.Item2,
                sessionResp.Item1.StartedOn,
                sessionResp.Item1.GetCardsToIgnoreList(),
                sessionResp.Item1.GetReviewPath());
        }
    }

    public record GetReviewSessionCommandHandlerResponse(
        bool IsNew,
        DateTime StartedOn,
        List<long> CardIdsToIgnore,
        Dictionary<Tab, int> ReviewPath)
    { }

    public record GetReviewSessionCommand(long UserId, bool StartNew = false) : IRequest<GetReviewSessionCommandHandlerResponse> { }
}

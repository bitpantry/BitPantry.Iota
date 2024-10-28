using BitPantry.Iota.Application.CRQS.Identity.Queries;
using BitPantry.Iota.Application.Service;
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
    public class GetActiveReviewSessionCommandHandler : IRequestHandler<GetReviewSessionCommand, GetReviewSessionCommandHandler>
    {
        private EntityDataContext _dbCtx;
        private ReviewSessionService _reviewSessionSvc;

        public GetActiveReviewSessionCommandHandler(EntityDataContext dbCtx, ReviewSessionService reviewSessionSvc)
        {
            _dbCtx = dbCtx;
            _reviewSessionSvc = reviewSessionSvc;
        }

        public async Task<GetReviewSessionCommandHandler> Handle(GetReviewSessionCommand request, CancellationToken cancellationToken)
        {
            var sessionResp = await _reviewSessionSvc.GetReviewSession(_dbCtx, request.UserId, request.StartNew);
            _dbCtx.SaveChanges();

            return new GetReviewSessionCommandHandler(
                sessionResp.Item2,
                sessionResp.Item1.StartedOn,
                sessionResp.Item1.GetCardsToIgnoreList());
        }
    }

    public record GetReviewSessionCommandHandler(
        bool IsNew,
        DateTime StartedOn,
        List<long> CardIdsToIgnore)
    { }

    public record GetReviewSessionCommand(long UserId, bool StartNew = false) : IRequest<GetReviewSessionCommandHandler> { }
}

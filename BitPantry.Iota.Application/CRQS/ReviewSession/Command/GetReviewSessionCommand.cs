using BitPantry.Iota.Application.CRQS.Identity.Queries;
using BitPantry.Iota.Application.Logic;
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
        private ReviewLogic _reviewLgc;

        public GetReviewSessionCommandHandler(EntityDataContext dbCtx, ReviewLogic reviewLgc)
        {
            _dbCtx = dbCtx;
            _reviewLgc = reviewLgc;
        }

        public async Task<GetReviewSessionCommandHandlerResponse> Handle(GetReviewSessionCommand request, CancellationToken cancellationToken)
        {
            var sessionResp = await _reviewLgc.GetReviewSessionCommand(_dbCtx, request.UserId, request.StartNew);
            await _dbCtx.SaveChangesAsync(cancellationToken);

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

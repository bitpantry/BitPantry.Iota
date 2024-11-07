using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Logic;
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

    public class GetNextCardForReviewQueryHandler : IRequestHandler<GetNextCardForReviewQuery, CardDto>
    {
        private EntityDataContext _dbCtx;
        private ReviewLogic _revLgc;

        public GetNextCardForReviewQueryHandler(EntityDataContext dbCtx, ReviewLogic revLgc)
        {
            _dbCtx = dbCtx;
            _revLgc = revLgc;
        }

        public async Task<CardDto> Handle(GetNextCardForReviewQuery request, CancellationToken cancellationToken)
        {
            var card = await _revLgc.GetNextCardForReviewCommand(_dbCtx, request.UserId, request.CurrentTab, request.CurrentCardOrder);
            await _dbCtx.SaveChangesAsync();
            return card;
        }
    }

    public record GetNextCardForReviewQuery(long UserId, Tab? CurrentTab = null, int CurrentCardOrder = 1) : IRequest<CardDto> { }

}

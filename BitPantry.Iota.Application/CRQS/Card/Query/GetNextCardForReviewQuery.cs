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
        private CardReviewService _revSvc;

        public GetNextCardForReviewQueryHandler(EntityDataContext dbCtx, CardReviewService revSvc)
        {
            _dbCtx = dbCtx;
            _revSvc = revSvc;
        }

        public async Task<GetNextCardForReviewQueryResponse> Handle(GetNextCardForReviewQuery request, CancellationToken cancellationToken)
        {
            var resp = await _revSvc.GetNextCardForReview(_dbCtx, request.UserId, request.LastDivider, request.LastCardIndex);

            if(resp == null)
                return null;

            // get bible

            var bible = resp.Verses.First().Chapter.Book.Testament.Bible;

            // resolve book name

            var bookName = BookNameDictionary.Get(
                bible.Classification,
                resp.Verses.First().Chapter.Book.Number);

            _ = await _dbCtx.SaveChangesAsync();

            return new GetNextCardForReviewQueryResponse
            (
                resp.Id,
                resp.AddedOn,
                resp.LastMovedOn,
                resp.LastReviewedOn,
                resp.Divider,
                resp.Order,
                new Passage(
                    resp.Verses.First().Chapter.Book.Testament.Bible.Id,
                    bookName.Value.Name,
                    resp.Verses.First().Chapter.Number,
                    resp.Verses.First().Number,
                    resp.Verses.Last().Chapter.Number,
                    resp.Verses.Last().Number,
                    resp.Verses.ToVerseDictionary()
                    )
            );
        }
    }

    public record GetNextCardForReviewQuery(long UserId, Divider? LastDivider = null, int LastCardIndex = 0) : IRequest<GetNextCardForReviewQueryResponse> { }

    public record GetNextCardForReviewQueryResponse(
        long Id,
        DateTime AddedOn,
        DateTime LastMovedOn,
        DateTime LastReviewedOn,
        Divider Divider,
        int Order,
        Passage Passage)
    { }
}

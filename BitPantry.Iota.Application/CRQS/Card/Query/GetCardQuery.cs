using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
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
    public class GetCardQueryHandler : IRequestHandler<GetCardQuery, GetCardQueryResponse>
    {
        private EntityDataContext _dbCtx;
        private CardService _cardSvc;

        public GetCardQueryHandler(EntityDataContext dbCtx, CardService cardSvc)
        {
            _dbCtx = dbCtx;
            _cardSvc = cardSvc;
        }

        public async Task<GetCardQueryResponse> Handle(GetCardQuery request, CancellationToken cancellationToken)
        {
            var resp = await _cardSvc.GetCard(_dbCtx, request.Id);

            // get bible

            var bible = resp.Verses.First().Chapter.Book.Testament.Bible;

            // resolve book name

            var bookName = BookNameDictionary.Get(
                bible.Classification,
                resp.Verses.First().Chapter.Book.Number);

            _ = await _dbCtx.SaveChangesAsync();

            return new GetCardQueryResponse
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

    public record GetCardQuery(long Id) : IRequest<GetCardQueryResponse> { }

    public record GetCardQueryResponse(
        long Id, 
        DateTime AddedOn,
        DateTime LastMovedOn,
        DateTime LastReviewedOn,
        Divider Divider,
        int Order,
        Passage Passage);

}

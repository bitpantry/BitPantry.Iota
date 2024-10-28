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
            var resp = request.Id > 0
                ? await _cardSvc.GetCard(_dbCtx, request.Id)
                : await _cardSvc.GetCard(_dbCtx, request.UserId, request.Divider, request.Order);

            _ = await _dbCtx.SaveChangesAsync();

            // get bible

            var bible = resp.Verses.First().Chapter.Book.Testament.Bible;

            // resolve book name

            var bookName = BookNameDictionary.Get(
                bible.Classification,
                resp.Verses.First().Chapter.Book.Number);

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

    public class GetCardQuery : IRequest<GetCardQueryResponse> 
    {
        public long Id { get; }
        public long UserId { get; }
        public Divider Divider { get; }
        public int Order { get; }

        public GetCardQuery(long id) => Id = id;

        public GetCardQuery(long userId, Divider divider, int order)
        {
            UserId = userId;
            Divider = divider;
            Order = order;
        }

    }

    public record GetCardQueryResponse(
        long Id, 
        DateTime AddedOn,
        DateTime LastMovedOn,
        DateTime LastReviewedOn,
        Divider Divider,
        int Order,
        Passage Passage);

}

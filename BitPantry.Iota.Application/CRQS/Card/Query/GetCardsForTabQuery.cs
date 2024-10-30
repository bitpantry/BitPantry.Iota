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
    public class GetCardsForTabQueryHandler : IRequestHandler<GetCardsForTabQuery, List<GetCardQueryResponse>>
    {
        private EntityDataContext _dbCtx;
        private CardService _cardSvc;

        public GetCardsForTabQueryHandler(EntityDataContext dbCtx, CardService cardSvc)
        {
            _dbCtx = dbCtx;
            _cardSvc = cardSvc;
        }

        public async Task<List<GetCardQueryResponse>> Handle(GetCardsForTabQuery request, CancellationToken cancellationToken)
        {
            var cards = await _cardSvc.GetCards(_dbCtx, request.UserId, request.Tab);

            _ = await _dbCtx.SaveChangesAsync();

            return cards.Select(GetDto).ToList();
        }

        private GetCardQueryResponse GetDto(GetCardResult res)
        {
            // get bible

            var bible = res.Verses.First().Chapter.Book.Testament.Bible;

            // resolve book name

            var bookName = BookNameDictionary.Get(
                bible.Classification,
                res.Verses.First().Chapter.Book.Number);

            return new GetCardQueryResponse
            (
                res.Id,
                res.AddedOn,
                res.LastMovedOn,
                res.LastReviewedOn,
                res.Tab,
                res.Order,
                new Passage(
                    res.Verses.First().Chapter.Book.Testament.Bible.Id,
                    bookName.Value.Name,
                    res.Verses.First().Chapter.Number,
                    res.Verses.First().Number,
                    res.Verses.Last().Chapter.Number,
                    res.Verses.Last().Number,
                    res.Verses.ToVerseDictionary()
                    )
            );
        }
    }

    public record GetCardsForTabQuery(long UserId, Tab Tab) : IRequest<List<GetCardQueryResponse>> { }
}

using System;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure;
using BitPantry.Iota.Infrastructure.Caching;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application.CRQS.Bible.Query;

public class GetBiblePassageQueryHandler : IRequestHandler<GetBiblePassageQuery, GetBiblePassageQueryResponse>
{
    private BibleService _bibleSvc;
    private EntityDataContext _dbCtx;

    public GetBiblePassageQueryHandler(BibleService bibleSvc, EntityDataContext dbCtx)
    {
        _bibleSvc = bibleSvc;
        _dbCtx = dbCtx;
    }

    public async Task<GetBiblePassageQueryResponse> Handle(GetBiblePassageQuery request, CancellationToken cancellationToken)
    {
        // get the passage

        var result = await _bibleSvc.GetPassage(request.BibleId, request.Address, true);

        if (result.Code != GetPassageResultCode.Ok)
            return new GetBiblePassageQueryResponse();

        // see if the user already has this card created

        var thumbprint = ThumbprintUtil.Generate(result.Passage.Verses.Select(v => v.Id).ToList());
        var userAlreadyHasCard = await _dbCtx.Cards.AnyAsync(c => c.Thumbprint.Equals(thumbprint) && c.UserId == request.UserId);
  
        // return the passage

        return new GetBiblePassageQueryResponse(
            true,
            userAlreadyHasCard,
            new Passage(
                result.Passage.BibleId,
                result.Passage.BookName,
                result.Passage.FromChapterNumber,
                result.Passage.FromVerseNumber,
                result.Passage.ToChapterNumber,
                result.Passage.ToVerseNumber,
                result.Passage.Verses.ToVerseDictionary()
            )
        );

    }


}

public record GetBiblePassageQuery(long UserId, string Address, long BibleId) : IRequest<GetBiblePassageQueryResponse>;

public record GetBiblePassageQueryResponse(
    bool IsValidAddress = false,
    bool IsAlreadyCreated = false,
    Passage Passage = null);

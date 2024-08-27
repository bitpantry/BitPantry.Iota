using System;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure;
using BitPantry.Iota.Infrastructure.Caching;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application.CRQS.Bible.Query;

public class GetBiblePassageQueryHandler : IRequestHandler<GetBiblePassageQuery, GetBiblePassageQueryResponse>
{
    private BibleService _bibleSvc;

    public GetBiblePassageQueryHandler(BibleService bibleSvc)
    {
        _bibleSvc = bibleSvc;
    }

    public async Task<GetBiblePassageQueryResponse> Handle(GetBiblePassageQuery request, CancellationToken cancellationToken)
    {
        var result = await _bibleSvc.GetPassage(request.BibleId, request.Address, true);

        if (result.Code != GetPassageResultCode.Ok)
            return new GetBiblePassageQueryResponse();

        return new GetBiblePassageQueryResponse(
            true,
            result.BibleId,
            result.BookName,
            result.ChapterNumber,
            result.FromVerseNumber,
            result.ToVerseNumber,
            result.Verses.ToDictionary(v => v.Number, v => v.Text)
        );

    }


}

public record GetBiblePassageQuery(string Address, long BibleId) : IRequest<GetBiblePassageQueryResponse>;

public record GetBiblePassageQueryResponse(
    bool IsValidAddress = false,
    long BibleId = 0, 
    string BookName = null,
    int ChapterNumber = 0,
    int FromVerseNumber = 0,
    int ToVerseNumber = 0,
    Dictionary<int, string> Verses = null);

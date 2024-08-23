using System;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure;
using BitPantry.Iota.Infrastructure.Caching;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application.CRQS.Bible.Query;

public class GetBiblePassageQueryHandler : IRequestHandler<GetBiblePassageQuery, GetBiblePassageQueryResponse>
{
    private EntityDataContext _dbCtx;
    private CacheService _cacheSvc;

    public GetBiblePassageQueryHandler(EntityDataContext dbCtx, CacheService cacheSvc)
    {
        _dbCtx = dbCtx;
        _cacheSvc = cacheSvc;
    }

    public async Task<GetBiblePassageQueryResponse> Handle(GetBiblePassageQuery request, CancellationToken cancellationToken)
    {
        // parse the raw address text

        var parser = new BiblePassageAddress(request.Address);
        if(!parser.IsValid)
            return new GetBiblePassageQueryResponse();

        // get the target bible translation

        var bibleList = await _dbCtx.Bibles.AsNoTracking().WithCaching(_cacheSvc).ToListAsync();
        var bible = request.BibleId == 0 ? bibleList.First() : bibleList.Single(b => b.Id == request.BibleId);

        // resolve the given book name to an actual book

        int minDistance = int.MaxValue;
        int matchingBookNumber = 0;

        var bookNameList = BookNameDictionary.Get(bible.Classification);

        foreach (var item in bookNameList)
        {
            int distance = item.Value.CalculateShortestLevenshteinDistance(parser.Book);

            if (distance < minDistance)
            {
                minDistance = distance;
                matchingBookNumber = item.Key;
            }
        }

        if(matchingBookNumber == 0)
            return new GetBiblePassageQueryResponse();

        // read verses

        var query = _dbCtx.Verses.AsNoTracking().Where(
            v => v.Chapter.Number == parser.Chapter && v.Chapter.Book.Number == matchingBookNumber && v.Chapter.Book.Testament.Bible.Id == bible.Id);

        if(parser.VerseEnd == 0)
            query = query.Where(v => v.Number == parser.VerseStart);
        else
            query = query.Where(v => v.Number >= parser.VerseStart && v.Number <= parser.VerseEnd);

        var verses = await query.WithCaching(_cacheSvc).ToListAsync();

        // return

        return new GetBiblePassageQueryResponse(
            true,
            bible.Id,
            bookNameList[matchingBookNumber].Name,
            parser.Chapter,
            parser.VerseStart,
            parser.VerseEnd,
            verses.ToDictionary(v => v.Number, v => v.Text)
        );

    }


}

public record GetBiblePassageQuery(string Address, long BibleId) : IRequest<GetBiblePassageQueryResponse>;

public record GetBiblePassageQueryResponse(
    bool IsValidAddress = false,
    long BibleId = 0, 
    string BookName = null,
    int ChapterNumber = 0,
    int VerseNumberFrom = 0,
    int VerseNumberTo = 0,
    Dictionary<int, string> Verses = null);

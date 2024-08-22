using System;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Data.Entity;
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
        var bible = bibleList.Single(b => b.Id == request.BibleId);

        // resolve the given book name to an actual book

        var bookList = BibleClassificationBookList.GetBookList(bible.Classification);
        var matchingBookNumber = bookList.MatchValue(parser.Book);

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
            bible.TranslationShortName,
            bible.TranslationLongName,
            matchingBookNumber,
            bookList[matchingBookNumber],
            parser.Chapter,
            parser.VerseStart,
            parser.VerseEnd,
            verses.ToDictionary(v => v.Number, v => v.Text)
        );

    }


}

public record GetBiblePassageQuery(long BibleId, string Address) : IRequest<GetBiblePassageQueryResponse>;

public record GetBiblePassageQueryResponse(
    bool IsValid = false,
    long BibleId = 0, 
    string TranslationShortName = null, 
    string TranslationLongName = null,
    int BookNumber = 0,
    string BookName = null,
    int ChapterNumber = 0,
    int VerseNumberFrom = 0,
    int VerseNumberTo = 0,
    Dictionary<int, string> Verses = null);

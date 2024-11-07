using System;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure;
using BitPantry.Iota.Infrastructure.Caching;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application.CRQS.Bible.Query;

public class GetBiblePassageQueryHandler : IRequestHandler<GetBiblePassageQuery, GetBiblePassageQueryResponse>
{
    private PassageLogic _bibleLgc;
    private EntityDataContext _dbCtx;
    private CacheService _cacheSvc;

    public GetBiblePassageQueryHandler(PassageLogic bibleLgc, EntityDataContext dbCtx, CacheService cacheSvc)
    {
        _bibleLgc = bibleLgc;
        _dbCtx = dbCtx;
        _cacheSvc = cacheSvc;
    }

    public async Task<GetBiblePassageQueryResponse> Handle(GetBiblePassageQuery request, CancellationToken cancellationToken)
    {
        // get the passage

        var result = await _bibleLgc.GetPassageQuery(_dbCtx, request.BibleId, request.Address, true);

        if (result.Code != GetPassageResultCode.Ok)
            return new GetBiblePassageQueryResponse();

        // see if the user already has this card created

        var userAlreadyHasCard = await _dbCtx.Cards.AnyAsync(c => c.Address.Equals(result.Passage.GetAddressString(false)) && c.UserId == request.UserId, cancellationToken: cancellationToken);

        // return the passage

        return new GetBiblePassageQueryResponse(
            true,
            userAlreadyHasCard,
            result.Passage
        );

    }


}

public record GetBiblePassageQuery(long UserId, string Address, long BibleId) : IRequest<GetBiblePassageQueryResponse>;

public record GetBiblePassageQueryResponse(
    bool IsValidAddress = false,
    bool IsAlreadyCreated = false,
    PassageDto Passage = null);

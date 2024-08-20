using System;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application.CRQS.Identity.Queries;

public class FindUserIdByEmailAddressQueryHandler : IRequestHandler<FindUserIdByEmailAddressQuery, long>
{
    private EntityDataContext _dbCtx;

    public FindUserIdByEmailAddressQueryHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }
    public async Task<long> Handle(FindUserIdByEmailAddressQuery request, CancellationToken cancellationToken)
    {
        return await _dbCtx.Users
            .AsNoTracking()
            .Where(u => u.EmailAddress.ToUpper().Equals(request.EmailAddress.ToUpper()))
            .Select(u => u.Id)
            .SingleOrDefaultAsync();
    }
}

public record FindUserIdByEmailAddressQuery(string EmailAddress) : IRequest<long> { }

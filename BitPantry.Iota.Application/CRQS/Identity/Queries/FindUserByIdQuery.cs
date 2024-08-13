using BitPantry.Iota.Application.CRQS.Identity.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Identity.Queries
{
    public class FindUserByIdQueryHandler : IRequestHandler<FindUserByIdQuery, FindUserByIdQueryResponse>
    {
        private readonly EntityDataContext _dbCtx;

        public FindUserByIdQueryHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }

        public async Task<FindUserByIdQueryResponse> Handle(FindUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _dbCtx.Users
                .AsNoTracking()
                .Where(u => u.Id == request.Id)
                .Select(u => new FindUserByIdQueryResponse(u))
                .SingleOrDefaultAsync(cancellationToken);
        }
    }

    public record FindUserByIdQuery(long Id) : IRequest<FindUserByIdQueryResponse> { }

    public class FindUserByIdQueryResponse : FindUserResponse
    {
        internal FindUserByIdQueryResponse(User user) : base(user) { }
    }
}

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
    public class FindUserByEmailQueryHandler : IRequestHandler<FindUserByEmailQuery, FindUserByEmailQueryResponse>
    {
        private readonly EntityDataContext _dbCtx;

        public FindUserByEmailQueryHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }

        public async Task<FindUserByEmailQueryResponse> Handle(FindUserByEmailQuery request, CancellationToken cancellationToken)
        {
            return await _dbCtx.Users
                .AsNoTracking()
                .Where(u => u.NormalizedEmailAddress.Equals(request.NormalizedEmailAddress.ToUpper()))
                .Select(u => new FindUserByEmailQueryResponse(u))
                .SingleOrDefaultAsync(cancellationToken);
        }
    }

    public record FindUserByEmailQuery(string NormalizedEmailAddress) : IRequest<FindUserByEmailQueryResponse> { }
   
    public class FindUserByEmailQueryResponse : FindUserResponse 
    {
        internal FindUserByEmailQueryResponse(User user) : base(user) { }
    }
}

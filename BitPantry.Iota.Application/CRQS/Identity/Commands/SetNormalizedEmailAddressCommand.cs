using BitPantry.Iota.Data.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Identity.Commands
{
    public class SetNormalizedEmailAddressCommandHandler : IRequestHandler<SetNormalizedEmailAddressCommand>
    {
        private readonly EntityDataContext _dbCtx;

        public SetNormalizedEmailAddressCommandHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }

        public async Task Handle(SetNormalizedEmailAddressCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbCtx.Users.FindAsync(request.Id, cancellationToken);
            user.NormalizedEmailAddress = request.NormalizedEmailAddress;
            await _dbCtx.SaveChangesAsync(cancellationToken);
        }
    }

    public record SetNormalizedEmailAddressCommand(long Id, string NormalizedEmailAddress) : IRequest { }
}

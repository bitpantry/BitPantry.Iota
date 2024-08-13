using BitPantry.Iota.Data.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Identity.Commands
{
    public class SetEmailAddressCommandHandler : IRequestHandler<SetEmailAddressCommand>
    {
        private readonly EntityDataContext _dbCtx;

        public SetEmailAddressCommandHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }

        public async Task Handle(SetEmailAddressCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbCtx.Users.FindAsync(request.Id, cancellationToken);
            user.EmailAddress = request.EmailAddress;
            await _dbCtx.SaveChangesAsync(cancellationToken);
        }
    }

    public record SetEmailAddressCommand(long Id, string EmailAddress) : IRequest { }
}

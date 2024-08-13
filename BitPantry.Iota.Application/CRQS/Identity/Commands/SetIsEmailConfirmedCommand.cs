using BitPantry.Iota.Data.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Identity.Commands
{
    public class SetIsEmailConfirmedCommandHandler : IRequestHandler<SetIsEmailConfirmedCommand>
    {
        private readonly EntityDataContext _dbCtx;

        public SetIsEmailConfirmedCommandHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }

        public async Task Handle(SetIsEmailConfirmedCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbCtx.Users.FindAsync(request.Id, cancellationToken);
            user.IsEmailConfirmed = request.IsEmailConfirmed;
            await _dbCtx.SaveChangesAsync(cancellationToken);
        }
    }

    public record SetIsEmailConfirmedCommand(long Id, bool IsEmailConfirmed) : IRequest { }
}

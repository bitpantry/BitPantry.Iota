using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Identity.Commands
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly EntityDataContext _dbCtx;

        public DeleteUserCommandHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            await _dbCtx.Users.Where(u => u.Id == request.UserId).ExecuteDeleteAsync(cancellationToken);
        }
    }

    public class DeleteUserCommand : IRequest
    {
        public long UserId { get; set; }
    }
}

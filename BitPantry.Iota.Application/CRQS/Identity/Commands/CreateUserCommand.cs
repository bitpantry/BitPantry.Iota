using BitPantry.Iota.Data.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Identity.Commands
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly EntityDataContext _dbCtx;

        public CreateUserCommandHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            _dbCtx.Users.Add(new User
            {
                EmailAddress = request.EmailAddress,
                Password = request.Password,
                PasswordHash = request.PasswordHash
            });

            _ = await _dbCtx.SaveChangesAsync(cancellationToken);
        }
    }

    public class CreateUserCommand : IRequest
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
    }
}

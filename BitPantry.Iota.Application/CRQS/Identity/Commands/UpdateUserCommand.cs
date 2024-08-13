using BitPantry.Iota.Data.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Identity.Commands
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
    {
        private readonly EntityDataContext _dbCtx;
        
        public UpdateUserCommandHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }

        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbCtx.Users.FindAsync(request.Id);
            
            user.EmailAddress = request.EmailAddress;
            user.NormalizedEmailAddress = request.NormalizedEmailAddress;
            user.Password = request.Password;
            user.PasswordHash = request.PasswordHash;
            user.IsEmailConfirmed = request.IsEmailConfirmed;

            await _dbCtx.SaveChangesAsync();
        }
    }

    public record UpdateUserCommand : IRequest
    {
        public required long Id { get; init; }
        public required string EmailAddress { get; init; }
        public required string NormalizedEmailAddress { get; init; }
        public required string Password { get; init; }
        public required string PasswordHash { get; init; }
        public required bool IsEmailConfirmed { get; init; }
    }
}

using System;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application.CRQS.Identity.Commands;

public class SignInUserCommandHandler : IRequestHandler<SignInUserCommand, long>
{
    private EntityDataContext _dbCtx;

    public SignInUserCommandHandler(EntityDataContext dbCtx) { _dbCtx = dbCtx; }

    public async Task<long> Handle(SignInUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbCtx.Users
            .Where(u => u.EmailAddress.ToUpper().Equals(request.EmailAddress.ToUpper()))
            .SingleOrDefaultAsync();

        if(user == null)
        {
            user = new User { EmailAddress = request.EmailAddress };
            _dbCtx.Users.Add(user);
        }

        user.LastLogin = DateTime.UtcNow;

        await _dbCtx.SaveChangesAsync();

        return user.Id;
    }
}

public record SignInUserCommand(string EmailAddress) : IRequest<long> { }

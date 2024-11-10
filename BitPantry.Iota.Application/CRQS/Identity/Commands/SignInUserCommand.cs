using System;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BitPantry.Iota.Application.CRQS.Identity.Commands;

public class SignInUserCommandHandler : IRequestHandler<SignInUserCommand, long>
{
    private ILogger<SignInUserCommandHandler> _logger;
    private EntityDataContext _dbCtx;

    public SignInUserCommandHandler(ILogger<SignInUserCommandHandler> logger, EntityDataContext dbCtx) 
    {
        _logger = logger;
        _dbCtx = dbCtx; 
    }

    public async Task<long> Handle(SignInUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("User signing in :: {EmailAddress}", request.EmailAddress);

        var user = await _dbCtx.Users
            .Where(u => u.EmailAddress.ToUpper().Equals(request.EmailAddress.ToUpper()))
            .SingleOrDefaultAsync();

        if(user == null)
        {
            _logger.LogDebug("Creating new user {EmailAddress}", request.EmailAddress);

            user = new User { EmailAddress = request.EmailAddress };
            _dbCtx.Users.Add(user);
        }

        user.LastLogin = DateTime.UtcNow;

        await _dbCtx.SaveChangesAsync();

        return user.Id;
    }
}

public record SignInUserCommand(string EmailAddress) : IRequest<long> { }

using BitPantry.Tabs.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BitPantry.Tabs.Application.Service
{
    public class IdentityService
    {
        private ILogger<IdentityService> _logger;
        private EntityDataContext _dbCtx;

        public IdentityService(ILogger<IdentityService> logger, EntityDataContext dbCtx)
        {
            _logger = logger;
            _dbCtx = dbCtx;
        }

        public async Task<long> SignInUser(string emailAddress, CancellationToken cancellationToken = default)
        {
            var user = await _dbCtx.Users
                .Where(u => u.EmailAddress.ToUpper().Equals(emailAddress.ToUpper()))
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                _logger.LogDebug("Creating new user {EmailAddress}", emailAddress);

                user = new User { EmailAddress = emailAddress, WorkflowType = Common.WorkflowType.Advanced };
                _dbCtx.Users.Add(user);
            }

            user.LastLogin = DateTime.UtcNow;

            await _dbCtx.SaveChangesAsync(cancellationToken);

            _dbCtx.ChangeTracker.Clear();

            _logger.LogDebug("User signed in :: {EmailAddress}, {Id}", emailAddress, user.Id);

            return user.Id;
        }

        public async Task<long> FindUserIdByEmailAddress(string emailAddress, CancellationToken cancellationToken)
            => await _dbCtx.Users
            .AsNoTracking()
            .Where(u => u.EmailAddress.ToUpper().Equals(emailAddress.ToUpper()))
            .Select(u => u.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }
}

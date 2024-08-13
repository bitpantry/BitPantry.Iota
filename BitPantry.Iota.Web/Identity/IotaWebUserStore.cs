using BitPantry.Iota.Application.CRQS.Identity.Commands;
using BitPantry.Iota.Application.CRQS.Identity.Queries;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace BitPantry.Iota.Web.Identity
{
    public class IotaWebUserStore : IUserStore<IotaWebUser>, IUserEmailStore<IotaWebUser>
    {
        private IMediator _med;

        public IotaWebUserStore(IMediator med) {
            _med = med;
        }
        public async Task<IdentityResult> CreateAsync(IotaWebUser user, CancellationToken cancellationToken)
        {
            await _med.Send(new CreateUserCommand
            {
                EmailAddress = user.EmailAddress,
                Password = user.Password,
                PasswordHash = user.PasswordHash
            }, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IotaWebUser user, CancellationToken cancellationToken)
        {
            await _med.Send(new DeleteUserCommand {  UserId = user.Id }, cancellationToken);
            return IdentityResult.Success;
        }

        public void Dispose() { /* do nothing */ }

        public async Task<IotaWebUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var resp = await _med.Send(new FindUserByEmailQuery(normalizedEmail), cancellationToken);
            if (resp == null) return null;
            return new IotaWebUser(resp);
        }

        public async Task<IotaWebUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var resp = await _med.Send(new FindUserByIdQuery(long.Parse(userId)), cancellationToken);
            if (resp == null) return null;
            return new IotaWebUser(resp);
        }

        public async Task<IotaWebUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var resp = await _med.Send(new FindUserByEmailQuery(normalizedUserName), cancellationToken);
            if (resp == null) return null;
            return new IotaWebUser(resp);
        }

        public async Task<string?> GetEmailAsync(IotaWebUser user, CancellationToken cancellationToken)
        {
            var resp = await _med.Send(new FindUserByIdQuery(user.Id), cancellationToken);
            if (resp == null) return null;
            return resp.EmailAddress;
        }

        public async Task<bool> GetEmailConfirmedAsync(IotaWebUser user, CancellationToken cancellationToken)
        {
            var resp = await _med.Send(new FindUserByIdQuery(user.Id), cancellationToken);
            if (resp == null) return false;
            return resp.IsEmailConfirmed;
        }

        public async Task<string?> GetNormalizedEmailAsync(IotaWebUser user, CancellationToken cancellationToken)
        {
            var resp = await _med.Send(new FindUserByIdQuery(user.Id), cancellationToken);
            if (resp == null) return null;
            return resp.NormalizedEmailAddress;
        }

        public async Task<string?> GetNormalizedUserNameAsync(IotaWebUser user, CancellationToken cancellationToken)
        {
            var resp = await _med.Send(new FindUserByIdQuery(user.Id), cancellationToken);
            if (resp == null) return null;
            return resp.NormalizedEmailAddress;
        }

        public async Task<string> GetUserIdAsync(IotaWebUser user, CancellationToken cancellationToken)
        {
            var resp = await _med.Send(new FindUserByEmailQuery(user.NormalizedEmailAddress), cancellationToken);
            if (resp == null) return "0";
            return resp.Id.ToString();
        }

        public async Task<string?> GetUserNameAsync(IotaWebUser user, CancellationToken cancellationToken)
        {
            var resp = await _med.Send(new FindUserByIdQuery(user.Id), cancellationToken);
            if (resp == null) return null;
            return resp.EmailAddress;
        }

        public async Task SetEmailAsync(IotaWebUser user, string? email, CancellationToken cancellationToken)
            => await _med.Send(new SetEmailAddressCommand(user.Id, email), cancellationToken);

        public async Task SetEmailConfirmedAsync(IotaWebUser user, bool confirmed, CancellationToken cancellationToken)
            => await _med.Send(new SetIsEmailConfirmedCommand(user.Id, confirmed), cancellationToken);

        public async Task SetNormalizedEmailAsync(IotaWebUser user, string? normalizedEmail, CancellationToken cancellationToken)
            => await _med.Send(new SetNormalizedEmailAddressCommand(user.Id, normalizedEmail), cancellationToken);

        public async Task SetNormalizedUserNameAsync(IotaWebUser user, string? normalizedName, CancellationToken cancellationToken)
            => await _med.Send(new SetNormalizedEmailAddressCommand(user.Id, normalizedName), cancellationToken);

        public async Task SetUserNameAsync(IotaWebUser user, string? userName, CancellationToken cancellationToken)
            => await _med.Send(new SetEmailAddressCommand(user.Id, userName), cancellationToken);

        public async Task<IdentityResult> UpdateAsync(IotaWebUser user, CancellationToken cancellationToken)
        {
            await _med.Send(new UpdateUserCommand
            {
                Id = user.Id,
                EmailAddress = user.EmailAddress,
                NormalizedEmailAddress = user.NormalizedEmailAddress,
                Password = user.Password,
                PasswordHash = user.PasswordHash,
                IsEmailConfirmed = user.IsEmailConfirmed
            }, cancellationToken);

            return IdentityResult.Success;
        }

    }
}

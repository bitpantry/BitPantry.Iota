
using BitPantry.Tabs.Application.Service;

namespace BitPantry.Tabs.Web.Cli
{
    public class CliKeyStore : IApiKeyStore
    {
        private UserService _userSvc;

        public CliKeyStore(UserService userSvc)
        {
            _userSvc = userSvc;
        }

        public Task<bool> TryGetClientIdByApiKey(string apiKey, out string clientId)
        {
            clientId = null;

            if (string.IsNullOrEmpty(apiKey))
                return Task.FromResult(false);

            var user = _userSvc.GetUser(apiKey).GetAwaiter().GetResult();
            if (user == null)
                return Task.FromResult(false);

            clientId = user.Id.ToString();

            return Task.FromResult(true);
        }
    }
}

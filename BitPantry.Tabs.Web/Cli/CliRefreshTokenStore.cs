using BitPantry.CommandLine.Remote.SignalR.Server.Authentication;
using BitPantry.Tabs.Application;
using BitPantry.Tabs.Data.Entity;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Tabs.Web.Cli
{
    public class CliRefreshTokenStore : IRefreshTokenStore
    {
        private EntityDataContext _dbCtx;

        public CliRefreshTokenStore(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task RevokeRefreshTokenAsync(string clientId)
        {
            await _dbCtx.UseConnection(CancellationToken.None, async (con, trans) =>
            {
                await con.ExecuteAsync("DELETE FROM CliRefreshTokens WHERE ClientId = @ClientId", new { clientId }, trans);
            });
        }

        public async Task StoreRefreshTokenAsync(string clientId, string refreshToken)
        {
            await RevokeRefreshTokenAsync(clientId);
            _dbCtx.CliRefreshTokens.Add(new CliRefreshToken { ClientId = clientId, RefreshToken = refreshToken });
            await _dbCtx.SaveChangesAsync();
        }

        public Task<bool> TryGetRefreshTokenAsync(string clientId, out string refreshToken)
        {
            refreshToken = null;

            var token = _dbCtx.CliRefreshTokens.AsNoTracking().FirstOrDefault(t => t.ClientId.Equals(clientId));
            if(token == null)
                return Task.FromResult(false);

            refreshToken = token.RefreshToken;
            return Task.FromResult(true);
        }
    }
}

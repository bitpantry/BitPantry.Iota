using BitPantry.CommandLine.Remote.SignalR.Server.Authentication;
using BitPantry.Tabs.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright.MSTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitPantry.Tabs.Application;
using Dapper;
using FluentAssertions;

namespace BitPantry.Tabs.Test.Playwright.Cli
{
    [TestClass]
    public class CliRefreshTokenTests : PageTest
    {
        [TestMethod]
        public async Task StoreRefreshToken_TokenStored()
        {
            using var scope = Fixture.Environment.ServiceProvider.CreateScope();

            var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
            var tknStore = scope.ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<IRefreshTokenStore>();

            dbCtx.CliRefreshTokens.ToList().Should().BeEmpty();

            await tknStore.StoreRefreshTokenAsync("storingNewTokenClientId", "testToken");
            var result = await tknStore.TryGetRefreshTokenAsync("storingNewTokenClientId", out var refreshToken);

            result.Should().BeTrue();
            refreshToken.Should().Be("testToken");
        }

        [TestMethod]
        public async Task RetrieveRefreshToken_DoesntExist_NoTokenReturned()
        {
            using var scope = Fixture.Environment.ServiceProvider.CreateScope();

            var tknStore = scope.ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<IRefreshTokenStore>();

            var result = await tknStore.TryGetRefreshTokenAsync("noExistId", out var refreshToken);

            result.Should().BeFalse();
            refreshToken.Should().BeNull();
        }

        [TestMethod]
        public async Task RetrieveRefreshToken_Exists_TokenReturned()
        {
            using var scope = Fixture.Environment.ServiceProvider.CreateScope();

            var tknStore = scope.ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<IRefreshTokenStore>();

            await tknStore.StoreRefreshTokenAsync("retrievedTokenClientId", "retrievedToken");

            var result = await tknStore.TryGetRefreshTokenAsync("retrievedTokenClientId", out var refreshToken);

            result.Should().BeTrue();
            refreshToken.Should().Be("retrievedToken");
        }

        [TestMethod]
        public async Task RevokeToken_TokenRevoked()
        {
            using var scope = Fixture.Environment.ServiceProvider.CreateScope();

            var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
            var tknStore = scope.ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<IRefreshTokenStore>();

            await tknStore.StoreRefreshTokenAsync("tokenToRevoke", "myRevokedToken");

            await tknStore.RevokeRefreshTokenAsync("tokenToRevoke");
            var result = await tknStore.TryGetRefreshTokenAsync("tokenToRevoke", out var refreshToken);

            result.Should().BeFalse();
            refreshToken.Should().BeNull();
        }

    }
}

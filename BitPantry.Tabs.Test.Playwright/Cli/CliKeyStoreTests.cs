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
    public class CliKeyStoreTests : PageTest
    {
        [TestMethod]
        public async Task GetApiKeyForUserThatHasOne_ApiKeyReturned()
        {
            var userId = await Fixture.Environment.CreateUser();

            using var scope = Fixture.Environment.ServiceProvider.CreateScope();

            var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
            var keyStore = scope.ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<IApiKeyStore>();

            await dbCtx.UseConnection(CancellationToken.None, async (con, trans) =>
            {
                await con.ExecuteAsync("UPDATE Users SET CliApiKey = @Key WHERE Id = @Id", new { Key = "testKey", Id = userId }, trans);
            });

            var result = await keyStore.TryGetClientIdByApiKey("testKey", out var clientId);

            result.Should().BeTrue();
            clientId.Should().Be(userId.ToString());
        }

        [TestMethod]
        public async Task GetApiKeyForUserThatDoesntHaveOne_NoApiKeyReturned()
        {
            var userId = await Fixture.Environment.CreateUser();

            using var scope = Fixture.Environment.ServiceProvider.CreateScope();
            var keyStore = scope.ServiceProvider.GetRequiredService<IApiKeyStore>();

            var result = await keyStore.TryGetClientIdByApiKey("nonExistantUserKey", out var clientId);

            result.Should().BeFalse();
            clientId.Should().BeNull();
        }

        [TestMethod]
        public async Task GetApiKeyForNonExistentUser_NoApiKeyReturned()
        {
            using var scope = Fixture.Environment.ServiceProvider.CreateScope();
            var keyStore = scope.ServiceProvider.GetRequiredService<IApiKeyStore>();

            var result = await keyStore.TryGetClientIdByApiKey("nonExistentKey", out var clientId);

            result.Should().BeFalse();
            clientId.Should().BeNull();
        }

        [TestMethod]
        public async Task GetApiKeyForUserWithNullApiKey_NoApiKeyReturned()
        {
            var userId = await Fixture.Environment.CreateUser();

            using var scope = Fixture.Environment.ServiceProvider.CreateScope();
            var keyStore = scope.ServiceProvider.GetRequiredService<IApiKeyStore>();

            var result = await keyStore.TryGetClientIdByApiKey(null, out var clientId);

            result.Should().BeFalse();
            clientId.Should().BeNull();
        }
    }
}

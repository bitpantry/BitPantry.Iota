using BitPantry.Tabs.Application;
using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Test.Application.Fixtures;
using Dapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BitPantry.Tabs.Test.Application.ServiceTests
{
    [Collection("env")]
    public class IdentityServiceTests 
    {
        ApplicationEnvironment _env;

        public IdentityServiceTests(AppEnvironmentFixture fixture)
        {
            _env = fixture.Environment;
        }

        [Fact]
        public async Task SignInNewUser_UserSignedIn()
        {
            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<IdentityService>();
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();

                var userCount = await dbCtx.UseConnection(CancellationToken.None, async conn =>
                {
                    return await conn.QuerySingleAsync<int>("SELECT COUNT(Id) FROM USERS WHERE EmailAddress = @EmailAddress", new { EmailAddress = "user@test.com" });
                });

                userCount.Should().Be(0);

                var userId = await svc.SignInUser("user@test.com");
            }

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                var user = await dbCtx.Users.Where(c => c.EmailAddress.Equals("user@test.com")).SingleAsync();

                user.EmailAddress.Should().Be("user@test.com");                
            }
        }

        [Fact]
        public async Task SignInExistingUser_UserSignedIn()
        {
            long userId = 0;

            using (var scope = _env.ServiceProvider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<IdentityService>();
                
                userId = await svc.SignInUser("testuser@test.com");
                var nextUserId = await svc.SignInUser("testuser@test.com");

                nextUserId.Should().Be(userId); // same user
            }
        }

    }
}

using BitPantry.Iota.Application;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Test.Fixtures;
using Dapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BitPantry.Iota.Test.ServiceTests
{
    [Collection("Services")]
    public class IdentityServiceTests 
    {
        ApplicationEnvironment _testEnv;

        public IdentityServiceTests(ApplicationEnvironmentCollectionFixture fixture)
        {
            _testEnv = fixture.Environment;
        }

        [Fact]
        public async Task SignInNewUser_UserSignedIn()
        {
            using (var scope = _testEnv.CreateDependencyScope())
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

            using (var scope = _testEnv.CreateDependencyScope())
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

            using (var scope = _testEnv.CreateDependencyScope())
            {
                var svc = scope.ServiceProvider.GetRequiredService<IdentityService>();
                
                userId = await svc.SignInUser("testuser@test.com");
                var nextUserId = await svc.SignInUser("testuser@test.com");

                nextUserId.Should().Be(userId); // same user
            }
        }

    }
}

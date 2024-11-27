using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Data.Entity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BitPantry.Iota.Test
{
    public class UserTests : IDisposable
    {
        private readonly TestEnvironment _testEnv;

        public UserTests()
        {
            _testEnv = TestEnvironment.Deploy();
        }

        [Fact]
        public async Task CreateUser_UserCreated()
        {
            using (var scope = _testEnv.CreateDependencyScope())
            {
                var userSvc = scope.ServiceProvider.GetRequiredService<IdentityService>();

                var userId = await userSvc.SignInUser("newUser@test.com");
                userId.Should().BeGreaterThan(0);

                var user = await scope.ServiceProvider.GetRequiredService<EntityDataContext>()
                    .Users.FindAsync(userId);
                
                user.Should().NotBeNull();

                user.Id.Should().Be(userId);
                user.EmailAddress.Should().Be("newUser@test.com");
                user.LastLogin.Date.Should().Be(DateTime.UtcNow.Date);
            }
        }

        public void Dispose()
        {
            _testEnv.Dispose();
        }
    }
}

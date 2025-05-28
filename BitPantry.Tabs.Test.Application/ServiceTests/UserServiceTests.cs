using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Test.Application.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BitPantry.Tabs.Test.Application.ServiceTests;

[Collection("env")]
public class UserServiceTests
{
    private readonly ApplicationEnvironment _env;

    public UserServiceTests(AppEnvironmentFixture fixture)
    {
        _env = fixture.Environment;
    }

    [Fact]
    public async Task GetUser_Nonexistent_ReturnsNull()
    {
        using var scope = _env.ServiceProvider.CreateScope();
        var svc = new UserService(scope.ServiceProvider.GetRequiredService<EntityDataContext>());

        var user = await svc.GetUser(9999);

        user.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByCliKey_ReturnsUser()
    {
        long userId;
        using(var scope = _env.ServiceProvider.CreateScope())
        {
            var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
            var user = new User { EmailAddress = "cli@test.com", WorkflowType = Common.WorkflowType.Basic, CliApiKey = "abc" };
            dbCtx.Users.Add(user);
            await dbCtx.SaveChangesAsync();
            userId = user.Id;
        }
        using(var scope = _env.ServiceProvider.CreateScope())
        {
            var svc = new UserService(scope.ServiceProvider.GetRequiredService<EntityDataContext>());
            var dto = await svc.GetUser("abc");
            dto.Should().NotBeNull();
            dto.Id.Should().Be(userId);
        }
    }
}

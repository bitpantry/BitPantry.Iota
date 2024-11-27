using BitPantry.Iota.Infrastructure.Settings;
using BitPantry.Iota.Web;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class TestUserMiddleware
{
    private readonly RequestDelegate _next;

    public TestUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppSettings settings, UserIdentity userIdentity)
    {
        if (!userIdentity.IsAuthenticated)
        {
            // Simulate a logged-in user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "testuser@example.com"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, settings.UseTestUserId.Value.ToString())
            };

            var identity = new ClaimsIdentity(claims, "FakeAuthentication");
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(principal);

            userIdentity.UserId = settings.UseTestUserId.Value;

        }

        await _next(context);
    }
}

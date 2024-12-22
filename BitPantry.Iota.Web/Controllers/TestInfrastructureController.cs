using BitPantry.Iota.Application.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BitPantry.Iota.Infrastructure.Settings;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;

namespace BitPantry.Iota.Web.Controllers
{
    [AllowAnonymous]
    public class TestInfrastructureController : Controller
    {
        private ILogger<TestInfrastructureController> _logger;
        private AppSettings _settings;
        private UserService _userService;
        private UserIdentity _userIdentity;

        public TestInfrastructureController(ILogger<TestInfrastructureController> logger, AppSettings settings, UserService userService, UserIdentity userIdentity)
        {
            _logger = logger;
            _settings = settings;
            _userService = userService;
            _userIdentity = userIdentity;
        }

        [Route("test/auth/{id:long}")]
        public async Task<IActionResult> Authenticate(long id)
        {
            if (!_settings.EnableTestInfrastructure)
                return NotFound();

            _logger.LogInformation("Authenticating test user by id {UserId}", id);

            var user = await _userService.GetUser(id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Email, user.EmailAddress)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            _userIdentity.UserId = id;

            return Route.RedirectTo<HomeController>(c => c.Delay(50, Url.Action<HomeController>(c => c.Index())));
        }

    }
}

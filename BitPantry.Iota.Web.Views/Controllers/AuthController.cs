using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BitPantry.Iota.Web.Controllers
{

    [AllowAnonymous]
    public class AuthController : Controller
    {
        private UserIdentity _userIdentity;
        private IdentityService _idSvc;
        private UserService _userService;
        private AppSettings _settings;

        public AuthController(UserIdentity userIdentity, IdentityService idSvc, UserService userSvc, AppSettings settings) 
        { 
            _userIdentity = userIdentity;
            _idSvc = idSvc;
            _userService = userSvc;
            _settings = settings;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task GoogleLogin()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            var emailClaim = result.Principal.Identities.First().Claims.Single(c => c.Type == ClaimTypes.Email);
            var id = await _idSvc.SignInUser(emailClaim.Value, HttpContext.RequestAborted);

            _userIdentity.UserId = id;

            return Route.RedirectTo<HomeController>(c => c.Delay(50, Url.Action<HomeController>(c => c.Index())));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> AuthenticateTestUser(long id)
        {
            if(!_settings.EnableTestInfrastructure)
                return NotFound();

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

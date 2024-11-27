using BitPantry.Iota.Application.Service;
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

        public AuthController(UserIdentity userIdentity, IdentityService idSvc) 
        { 
            _userIdentity = userIdentity;
            _idSvc = idSvc;
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
    }
}

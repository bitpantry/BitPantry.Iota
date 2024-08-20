using BitPantry.Iota.Application.CRQS.Identity.Commands;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Security.Claims;

namespace BitPantry.Iota.Web.Controllers
{

    [AllowAnonymous]
    public class AuthController : Controller
    {
        private IMediator _med;
        private UserIdentity _userIdentity;

        public AuthController(IMediator med, UserIdentity userIdentity) 
        { 
            _med = med;
            _userIdentity = userIdentity;
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
            var id = await _med.Send(new SignInUserCommand(emailClaim.Value));
            _userIdentity.UserId = id;

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}

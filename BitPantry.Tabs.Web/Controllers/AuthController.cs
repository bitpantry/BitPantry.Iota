using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BitPantry.Tabs.Web.Controllers
{

    [AllowAnonymous]
    public class AuthController : Controller
    {
        private ILogger<AuthController> _logger;
        private UserIdentity _userIdentity;
        private IdentityService _idSvc;

        public AuthController(ILogger<AuthController> logger, UserIdentity userIdentity, IdentityService idSvc) 
        { 
            _logger = logger;
            _userIdentity = userIdentity;
            _idSvc = idSvc;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task GoogleLogin(string returnUrl = null)
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                BuildAuthenticationProperties(nameof(GoogleLoginResponse), returnUrl));
        }

        public async Task MicrosoftLogin(string returnUrl = null)
        {
            await HttpContext.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme,
                BuildAuthenticationProperties(nameof(MicrosoftLoginResponse), returnUrl));
        }

        private AuthenticationProperties BuildAuthenticationProperties(string authRedirectUrl, string returnUrl)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(authRedirectUrl)
            };

            if (!string.IsNullOrEmpty(returnUrl))
                properties.Items["returnUrl"] = returnUrl;

            return properties;
        }

        [Route("/auth/microsoft-login-response")]
        public async Task<IActionResult> MicrosoftLoginResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var emailClaim = result.Principal.Identities.First().Claims.Single(c => c.Type == ClaimTypes.Email);
            var id = await _idSvc.SignInUser(emailClaim.Value, HttpContext.RequestAborted);

            _userIdentity.UserId = id;

            var returnUrl = result.Properties.Items.ContainsKey("returnUrl") 
                ? result.Properties.Items["returnUrl"] 
                : Url.Action<HomeController>(c => c.Index());

            return Redirect(returnUrl);
        }

        [Route("/auth/google-login-response")]
        public async Task<IActionResult> GoogleLoginResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            var emailClaim = result.Principal.Identities.First().Claims.Single(c => c.Type == ClaimTypes.Email);
            var id = await _idSvc.SignInUser(emailClaim.Value, HttpContext.RequestAborted);

            _userIdentity.UserId = id;

            var returnUrl = result.Properties.Items.ContainsKey("returnUrl")
                ? result.Properties.Items["returnUrl"]
                : Url.Action<HomeController>(c => c.Index());

            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }


    }
}

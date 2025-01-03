using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BitPantry.Tabs.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWorkflowService _workflowSvc;
        private readonly CardService _cardSvc;
        private readonly UserIdentity _identity;
        private readonly UserTimeService _userTimeSvc;

        public HomeController(ILogger<HomeController> logger, IWorkflowService workflowSvc, CardService cardSvc, UserIdentity identity, UserTimeService userTimeSvc)
        {
            _logger = logger;
            _workflowSvc = workflowSvc;
            _cardSvc = cardSvc;
            _identity = identity;
            _userTimeSvc = userTimeSvc;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {

                var totalCardCount = await _cardSvc.GetCardCountForUser(_identity.UserId, HttpContext.RequestAborted);
                var path = await _workflowSvc.GetReviewPath(_identity.UserId, _userTimeSvc.GetCurrentUserLocalTime(), HttpContext.RequestAborted);

                return View(new HomeModel(
                    totalCardCount > 0,
                    path.CardsToReviewCount
                ));

            }

            return View(new HomeModel(false, 0));
        }

        [AllowAnonymous]
        public IActionResult GetStarted()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Delay(int delay, string redirectUrl = "/")
        {
            return View(new { Delay = delay, RedirectUrl = redirectUrl });
        }

        [AllowAnonymous]
        public IActionResult AddTimezoneCookie(string redirectUrl)
        {
            return View(new { RedirectUrl =  redirectUrl });
        }

        [AllowAnonymous]
        public IActionResult Documentation()
            => View(nameof(Documentation));

        
    }
}

using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BitPantry.Iota.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CardService _cardSvc;
        private readonly ReviewService _reviewSvc;
        private readonly UserIdentity _identity;
        private readonly UserTimeService _userTimeSvc;

        public HomeController(ILogger<HomeController> logger, CardService cardSvc, ReviewService reviewSvc, UserIdentity identity, UserTimeService userTimeSvc)
        {
            _logger = logger;
            _cardSvc = cardSvc;
            _reviewSvc = reviewSvc;
            _identity = identity;
            _userTimeSvc = userTimeSvc;
        }

        public async Task<IActionResult> Index()
        {
            var totalCardCount = await _cardSvc.GetUserCardCount(_identity.UserId, HttpContext.RequestAborted);
            var path = await _reviewSvc.GetReviewPath(_identity.UserId, _userTimeSvc.GetCurrentUserLocalTime(), HttpContext.RequestAborted);

            return View(new HomeModel(
                totalCardCount > 0,
                path.CardsToReviewCount
            ));
        }

        public IActionResult GetStarted()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Delay(int delay, string redirectUrl)
        {
            return View(new { Delay = delay, RedirectUrl = redirectUrl });
        }

        [AllowAnonymous]
        public IActionResult AddTimezoneCookie(string redirectUrl)
        {
            return View(new { RedirectUrl =  redirectUrl });
        }

        public IActionResult Documentation()
            => View(nameof(Documentation));

        
    }
}

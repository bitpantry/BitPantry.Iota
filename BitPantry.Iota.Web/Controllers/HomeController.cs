using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.CRQS.Card.Query;
using BitPantry.Iota.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace BitPantry.Iota.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMediator _med;
        private readonly UserIdentity _identity;
        private readonly UserTimeService _userTimeSvc;

        public HomeController(ILogger<HomeController> logger, IMediator med, UserIdentity identity, UserTimeService userTimeSvc)
        {
            _logger = logger;
            _med = med;
            _identity = identity;
            _userTimeSvc = userTimeSvc;
        }

        public async Task<IActionResult> Index()
        {
            var homeCardInfo = await _med.Send(new GetHomeCardInfoQuery(_identity.UserId, _userTimeSvc.GetCurrentUserLocalTime()));

            return View(new HomeModel(
                homeCardInfo.TotalCardCount > 0,
                homeCardInfo.CardsToReviewTodayCount
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

        public IActionResult Feedback()
            => View(nameof(Feedback));

        
    }
}

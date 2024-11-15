using BitPantry.Iota.Application;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BitPantry.Iota.Web.Controllers
{
    public class ReviewController : Controller
    {
        private readonly CardService _cardSvc;
        private readonly ReviewService _reviewSvc;
        private UserIdentity _identity;
        private UserTimeService _userTime;
        private ILogger<ReviewController> _logger;

        public ReviewController(CardService cardSvc, ReviewService reviewSvc, UserIdentity identity, UserTimeService userTime, ILogger<ReviewController> logger)
        {
            _cardSvc = cardSvc;
            _reviewSvc = reviewSvc;
            _identity = identity;
            _userTime = userTime;
            _logger = logger;
        }

        [Route("review")]
        public async Task<IActionResult> Index()
        {
            var path = await _reviewSvc.GetReviewPath(_identity.UserId, _userTime.GetCurrentUserLocalTime(), HttpContext.RequestAborted);

            if (!path.Path.Any(p => p.Value > 0))
                return NoCards();

            // get next review step

            return await Review(Tab.Daily, 1);
        }

        [Route("review/{tab:enum}/{ord:int?}")]
        public async Task<IActionResult> Review(Tab tab, int ord = 1)
        {
            var path = await _reviewSvc.GetReviewPath(_identity.UserId, _userTime.GetCurrentUserLocalTime(), HttpContext.RequestAborted);
            var card = await _cardSvc.GetCard(_identity.UserId, tab, ord, HttpContext.RequestAborted);

            return View(nameof(Review), new ReviewModel(path.Path, tab, ord, card.ToModel()));
        }

        [Route("review/next/{currentTab:enum}/{currentOrd:int}")]
        public async Task<IActionResult> Next(Tab currentTab, int currentOrd)
        {
            await _cardSvc.MarkCardAsReviewed(_identity.UserId, currentTab, currentOrd, HttpContext.RequestAborted);


            var path = await _reviewSvc.GetReviewPath(_identity.UserId, _userTime.GetCurrentUserLocalTime(), HttpContext.RequestAborted);
            var helper = new ReviewPathHelper(path.Path);

            var nextStep = helper.GetNextStep(currentTab, currentOrd);

            if (nextStep == null)
                return Done();

            var nextTab = nextStep.Value.Key;
            var nextOrd = nextStep.Value.Value;

            var card = await _cardSvc.GetCard(_identity.UserId, nextTab, nextOrd, HttpContext.RequestAborted);

            return View(nameof(Review), new ReviewModel(path.Path, nextTab, nextOrd, card.ToModel()));
        }

        [Route("review/promote/{id:long}")]
        public async Task<IActionResult> Promote(long id)
        {

            // promote the daily card

            await _cardSvc.PromoteDailyCard(id, HttpContext.RequestAborted);

            // go to next step in review

            return await Review(Tab.Daily, 1);
        }

        public IActionResult Done()
        {
            return View(nameof(Done));
        }

        public IActionResult NoCards()
        {
            return View(nameof(NoCards));
        }





    }
}

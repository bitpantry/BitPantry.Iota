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
        private readonly IWorkflowService _workflowSvc;
        private UserIdentity _identity;
        private UserTimeService _userTime;
        private ILogger<ReviewController> _logger;

        public ReviewController(CardService cardSvc, IWorkflowService workflowSvc, UserIdentity identity, UserTimeService userTime, ILogger<ReviewController> logger)
        {
            _cardSvc = cardSvc;
            _workflowSvc = workflowSvc;
            _identity = identity;
            _userTime = userTime;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var path = await _workflowSvc.GetReviewPath(_identity.UserId, _userTime.GetCurrentUserLocalTime(), HttpContext.RequestAborted);

            if (!path.Path.Any(p => p.Value > 0))
                return NoCards();

            // get next review step

            return await Index(Tab.Daily, 1);
        }

        [Route("review/{tab:enum}/{ord:int?}")]
        public async Task<IActionResult> Index(Tab tab, int ord = 1)
        {
            await _cardSvc.MarkAsReviewed(_identity.UserId, tab, ord, HttpContext.RequestAborted);

            var path = await _workflowSvc.GetReviewPath(_identity.UserId, _userTime.GetCurrentUserLocalTime(), HttpContext.RequestAborted);
            var card = await _cardSvc.GetCard(_identity.UserId, tab, ord, HttpContext.RequestAborted);

            var helper = new ReviewPathHelper(path.Path);
            var nextStep = helper.GetNextStep(tab, ord);

            var nextUrl = nextStep == null
                ? Url.Action<ReviewController>(c => c.Done())
                : Url.Action<ReviewController>(c => c.Index(nextStep.Value.Key, nextStep.Value.Value));

            return View(nameof(Index), new ReviewModel(path.Path, tab, ord, card.ToModel(), nextUrl));
        }

        [Route("review/promote/{id:long}")]
        public async Task<IActionResult> Promote(long id)
        {
            // promote the daily card

            await _workflowSvc.PromoteCard(id, HttpContext.RequestAborted);

            // go to next step in review

            return Route.RedirectTo<ReviewController>(c => c.Index(Tab.Daily, 1));
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

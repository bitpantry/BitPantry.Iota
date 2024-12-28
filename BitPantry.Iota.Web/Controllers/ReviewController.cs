using BitPantry.Iota.Application;
using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace BitPantry.Iota.Web.Controllers
{
    public class ReviewController : Controller
    {
        private readonly CardService _cardSvc;
        private readonly IWorkflowService _workflowSvc;
        private readonly CurrentUser _currentUser;
        private ILogger<ReviewController> _logger;

        public ReviewController(CardService cardSvc, IWorkflowService workflowSvc, CurrentUser currentUser, ILogger<ReviewController> logger)
        {
            _cardSvc = cardSvc;
            _workflowSvc = workflowSvc;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var path = await _workflowSvc.GetReviewPath(_currentUser.UserId, _currentUser.CurrentUserLocalTime, HttpContext.RequestAborted);

            if (!path.Path.Any(p => p.Value > 0))
                return Route.RedirectTo<ReviewController>(c => c.NoCards());

            // get next review step

            return Route.RedirectTo<ReviewController>(c => c.Index(path.Path.First().Key, 1));
        }

        [Route("review/{tab:enum}/{ord:int?}")]
        public async Task<IActionResult> Index(Tab tab, int ord = 1)
        {
            // redirect to first available card in path if card for given tab / order doesn't exist

            var path = await _workflowSvc.GetReviewPath(_currentUser.UserId, _currentUser.CurrentUserLocalTime, HttpContext.RequestAborted);
            if (!path.Path.ContainsKey(tab))
                return Route.RedirectTo<ReviewController>(c => c.Index(path.Path.First().Key, 1));

            // load card

            await _cardSvc.MarkAsReviewed(_currentUser.UserId, tab, ord, HttpContext.RequestAborted);
            
            var card = await _cardSvc.GetCard(_currentUser.UserId, tab, ord, HttpContext.RequestAborted);

            var helper = new ReviewPathHelper(path.Path);
            var nextStep = helper.GetNextStep(tab, ord);

            var nextUrl = nextStep == null
                ? Url.Action<ReviewController>(c => c.Done())
                : Url.Action<ReviewController>(c => c.Index(nextStep.Value.Key, nextStep.Value.Value));

            return View(nameof(Index), new ReviewModel(path.Path, tab, ord, card.ToModel(), nextUrl, _currentUser.WorkflowType));
        }

        [Route("review/promote/{id:long}")]
        public async Task<IActionResult> Promote(long id)
        {
            // promote the daily card

            await _workflowSvc.PromoteCard(id, HttpContext.RequestAborted);

            // go to next step in review

            var path = await _workflowSvc.GetReviewPath(_currentUser.UserId, _currentUser.CurrentUserLocalTime, HttpContext.RequestAborted);

            if (!path.Path.Any(p => p.Value > 0))
                return Route.RedirectTo<ReviewController>(c => c.NoCards());

            return Route.RedirectTo<ReviewController>(c => c.Index(path.Path.First().Key, 1));
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

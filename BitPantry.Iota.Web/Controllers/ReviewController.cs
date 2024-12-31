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
        private readonly TabsService _tabSvc;
        private readonly IWorkflowService _workflowSvc;
        private readonly CurrentUser _currentUser;
        private ILogger<ReviewController> _logger;

        public ReviewController(CardService cardSvc, TabsService tabSvc, IWorkflowService workflowSvc, CurrentUser currentUser, ILogger<ReviewController> logger)
        {
            _cardSvc = cardSvc;
            _tabSvc = tabSvc;
            _workflowSvc = workflowSvc;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var path = await _workflowSvc.GetReviewPath(_currentUser.UserId, _currentUser.CurrentUserLocalTime, HttpContext.RequestAborted);
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

            if (_currentUser.WorkflowType == WorkflowType.Basic)
                await _cardSvc.MarkAsReviewed(_currentUser.UserId, tab, ord, HttpContext.RequestAborted);

            var card = await _cardSvc.GetCard(_currentUser.UserId, tab, ord, HttpContext.RequestAborted);

            // build the next button url

            var helper = new ReviewPathHelper(path.Path);
            var nextStep = helper.GetNextStep(tab, ord);

            var nextUrl = nextStep == null
                ? Url.Action<ReviewController>(c => c.Done())
                : Url.Action<ReviewController>(c => c.Index(nextStep.Value.Key, nextStep.Value.Value));

            // if on daily tab, evaluate if should show bring up from queue button

            var queueCardCount = await _tabSvc.GetCardCountForTab(_currentUser.UserId, Tab.Queue, HttpContext.RequestAborted);

            // return

            return View(nameof(Index), new ReviewModel(path.Path, tab, ord, card?.ToModel(), nextUrl, _currentUser.WorkflowType, queueCardCount));
        }

        [Route("review/promote/{id:long}")]
        public async Task<IActionResult> Promote(long id)
        {
            var card = await _cardSvc.GetCard(id);

            // promote the daily card

            await _workflowSvc.PromoteCard(id, HttpContext.RequestAborted);

            // go to next step in review

            var path = await _workflowSvc.GetReviewPath(_currentUser.UserId, _currentUser.CurrentUserLocalTime, HttpContext.RequestAborted);
            var pathHelper = new ReviewPathHelper(path.Path);

            var nextStep = pathHelper.GetNextStep(card.Tab, (int) card.RowNumber - 1);

            if(nextStep == null)
                return Route.RedirectTo<ReviewController>(c => c.Done());
            else
                return Route.RedirectTo<ReviewController>(c => c.Index(nextStep.Value.Key, nextStep.Value.Value));
        }

        public async Task<IActionResult> GetNextQueueCard()
        {
            var queueCard = await _cardSvc.GetNextQueueCard(_currentUser.UserId, HttpContext.RequestAborted);
            await _workflowSvc.MoveCard(queueCard.Id, Tab.Daily, true, HttpContext.RequestAborted);

            return Route.RedirectTo<ReviewController>(r => r.Index());
        }

        [Route("review/gotit/{id:long}")]
        public async Task<IActionResult> GotIt(long id)
        {
            // mark card as reviewed

            await _cardSvc.MarkAsReviewed(id, HttpContext.RequestAborted);

            // go to next step in review

            var card = await _cardSvc.GetCard(id, HttpContext.RequestAborted);
            var path = await _workflowSvc.GetReviewPath(_currentUser.UserId, _currentUser.CurrentUserLocalTime, HttpContext.RequestAborted);

            var nextTab = path.GetNextStep(card.Tab);

            if(nextTab == null)
                return Route.RedirectTo<ReviewController>(c => c.Done());

            return Route.RedirectTo<ReviewController>(c => c.Index(nextTab.Value, 1));
        }

        public IActionResult Done()
        {
            return View(nameof(Done));
        }

        //public IActionResult NoCards()
        //{
        //    return View(nameof(NoCards));
        //}





    }
}

using BitPantry.Iota.Application;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.CRQS.Card.Query;
using BitPantry.Iota.Application.CRQS.Review.Query;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.Latency;
using System.Security.Cryptography;

namespace BitPantry.Iota.Web.Controllers
{
    public class ReviewController : Controller
    {
        private IMediator _med;
        private UserIdentity _identity;
        private UserTimeService _userTime;
        private ILogger<ReviewController> _logger;

        public ReviewController(IMediator med, UserIdentity identity, UserTimeService userTime, ILogger<ReviewController> logger)
        {
            _med = med;
            _identity = identity;
            _userTime = userTime;
            _logger = logger;
        }

        [Route("review")]
        public async Task<IActionResult> Index()
        {
            var path = await _med.Send(new GetReviewPathQuery(_identity.UserId, _userTime.GetCurrentUserLocalTime()));

            if (!path.Any(p => p.Value > 0))
                return NoCards();

            // get next review step

            return await Review(Tab.Daily, 1);
        }

        [Route("review/{tab:enum}/{ord:int?}")]
        public async Task<IActionResult> Review(Tab tab, int ord = 1)
        {
            var path = await _med.Send(new GetReviewPathQuery(_identity.UserId, _userTime.GetCurrentUserLocalTime()));
            var card = await _med.Send(new GetCardQuery(_identity.UserId, tab, ord));

            return View(nameof(Review), new ReviewModel(path, tab, ord, card.ToModel()));
        }

        [Route("review/next/{currentTab:enum}/{currentOrd:int}")]
        public async Task<IActionResult> Next(Tab currentTab, int currentOrd)
        {
            await _med.Send(new MarkCardAsReviewedCommand(_identity.UserId, currentTab, currentOrd));

            var path = await _med.Send(new GetReviewPathQuery(_identity.UserId, _userTime.GetCurrentUserLocalTime()));
            var helper = new ReviewPathHelper(path);

            var nextStep = helper.GetNextStep(currentTab, currentOrd);

            if (nextStep == null)
                return Done();

            var nextTab = nextStep.Value.Key;
            var nextOrd = nextStep.Value.Value;

            var card = await _med.Send(new GetCardQuery(_identity.UserId, nextTab, nextOrd));

            return View(nameof(Review), new ReviewModel(path, nextTab, nextOrd, card.ToModel()));
        }

        [Route("review/promote/{id:long}")]
        public async Task<IActionResult> Promote(long id)
        {

            // promote the daily card

            await _med.Send(new PromoteDailyCardCommand(_identity.UserId, id));

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

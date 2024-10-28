using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.CRQS.Card.Query;
using BitPantry.Iota.Application.CRQS.ReviewSession.Command;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace BitPantry.Iota.Web.Controllers
{
    public class ReviewController : Controller
    {
        private IMediator _med;
        private UserIdentity _identity;
        private ILogger<ReviewController> _logger;

        public ReviewController(IMediator med, UserIdentity identity, ILogger<ReviewController> logger)
        {
            _med = med;
            _identity = identity;
            _logger = logger;
        }

        [Route("review")]
        public async Task<IActionResult> Index()
        {
            // reset the session if no div or ord specified

            var session = await _med.Send(new GetReviewSessionCommand(_identity.UserId, true));
            if (!session.ReviewPath.Any(p => p.Value > 0))
                return RedirectToAction(nameof(NoCards));

            // get next review step

            return await GetNextReviewStepRedirect();
        }

        [Route("review/{div:enum}/{ord:int}")]
        public async Task<IActionResult> Review(Divider div, int ord)
        {
            // ensure session

            _ = await _med.Send(new GetReviewSessionCommand(_identity.UserId));

            // get the card

            var resp = await _med.Send(new GetCardQuery(_identity.UserId, div, ord));

            return View(new CardModel(
                resp.Id,
                resp.AddedOn,
                resp.LastMovedOn,
                resp.LastReviewedOn,
                resp.Divider,
                resp.Order,
                new PassageModel(
                    resp.Passage.BibleId,
                    resp.Passage.BookName,
                    resp.Passage.FromChapterNumber,
                    resp.Passage.FromVerseNumber,
                    resp.Passage.ToChapterNumber,
                    resp.Passage.ToVerseNumber,
                    resp.Passage.Address,
                    resp.Passage.Verses)
                ));
        }

        [Route("next/{currentDiv:enum}/{currentOrd:int}")]
        public async Task<IActionResult> Next(Divider currentDiv, int currentOrd)
        {
            // ensure session

            _ = await _med.Send(new GetReviewSessionCommand(_identity.UserId));

            // mark the current card as reviewed

            await _med.Send(new MarkCardAsReviewedCommand(_identity.UserId, currentDiv, currentOrd));

            // go to next step in review

            return await GetNextReviewStepRedirect(currentDiv, currentOrd);
        }

        [Route("promote/{id:long}")]
        public async Task<IActionResult> Promote(long id)
        {
            // ensure session

            _ = await _med.Send(new GetReviewSessionCommand(_identity.UserId));

            // promote the daily card

            await _med.Send(new PromoteDailyCardCommand(_identity.UserId, id));

            // go to next step in review

            return await GetNextReviewStepRedirect();
        }

        public IActionResult Done()
        {
            return View();
        }

        public IActionResult NoCards()
        {
            return View();
        }

        private async Task<IActionResult> GetNextReviewStepRedirect(Divider currentDiv = Divider.Queue, int currentOrder = 1)
        {
            var resp = await _med.Send(new GetNextCardForReviewQuery(_identity.UserId, currentDiv, currentOrder));

            if (resp == null)
                return RedirectToAction(nameof(Done));

            return RedirectToAction(nameof(Review), new { div = resp.Divider, ord = resp.Order });
        }



    }
}

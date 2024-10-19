using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.CRQS.Card.Query;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BitPantry.Iota.Web.Controllers
{
    public class ReviewController : Controller
    {
        private IMediator _med;
        private SessionState _session;
        private UserIdentity _identity;

        public ReviewController(IMediator med, SessionState session, UserIdentity identity)
        {
            _med = med;
            _session = session;
            _identity = identity;
        }

        public async Task<IActionResult> Index()
        {

            if (_session.ReviewProgress == null)
                _session.ReviewProgress = new ReviewProgress();

            var resp = await _med.Send(new GetNextCardForReviewQuery(_identity.UserId));

            if (resp == null)
                return View("NoCardsToReview");

            return View(BuildCardModel(resp));
        }

        public async Task<IActionResult> Next(Divider? div, int ord)
        {
            if (_session.ReviewProgress == null)
                return RedirectToAction("Index");

            GetNextCardForReviewQueryResponse resp = null;
            
            do
            {

                resp = await _med.Send(new GetNextCardForReviewQuery(_identity.UserId, div, ord));

            } while (resp != null && _session.ReviewProgress.CardIdsPromoted.Contains(resp.Id));

            if (resp == null)
                return View("ReviewComplete");

            return View(BuildCardModel(resp));
        }

        public async Task<IActionResult> Promote(long id)
        {
            if (_session.ReviewProgress == null)
                return RedirectToAction("Index");

            await _med.Send(new PromoteDailyCardCommand(id));

            _session.ReviewProgress.CardIdsPromoted.Add(id);

            return RedirectToAction("Index");
        }

        private CardModel BuildCardModel(GetNextCardForReviewQueryResponse resp)
        {
            return new CardModel(
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
                );
        }
    }
}

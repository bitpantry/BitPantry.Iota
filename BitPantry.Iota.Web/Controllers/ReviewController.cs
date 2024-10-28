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

        public async Task<IActionResult> Index(bool resetSession = true)
        {
            _ = await _med.Send(new GetReviewSessionCommand(_identity.UserId, resetSession));
            var resp = await _med.Send(new GetNextCardForReviewQuery(_identity.UserId));

            if (resp == null)
                return View("NoCardsToReview");

            return View(BuildCardModel(resp));
        }

        public async Task<IActionResult> Next(Divider? div, int ord)
        {
            if(div.HasValue)
                await _med.Send(new MarkCardAsReviewedCommand(_identity.UserId, div.Value, ord));
            
            var session = await _med.Send(new GetReviewSessionCommand(_identity.UserId));

            if(session.IsNew)
                return RedirectToAction("Index");

            var resp = await _med.Send(new GetNextCardForReviewQuery(_identity.UserId, div, ord));

            if (resp == null)
                return View("ReviewComplete");

            return View("Index", BuildCardModel(resp));
        }

        public async Task<IActionResult> Promote(long id)
        {
            var session = await _med.Send(new GetReviewSessionCommand(_identity.UserId));

            if (session.IsNew)
                return RedirectToAction("Index");

            await _med.Send(new PromoteDailyCardCommand(_identity.UserId, id));

            var resp = await _med.Send(new GetNextCardForReviewQuery(_identity.UserId, Divider.Queue));

            if (resp == null)
                return View("ReviewComplete");

            return View("Index", BuildCardModel(resp));
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

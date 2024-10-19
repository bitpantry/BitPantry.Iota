using BitPantry.Iota.Application.CRQS.Bible.Query;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.CRQS.Card.Query;
using BitPantry.Iota.Application.CRQS.Set.Query;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace BitPantry.Iota.Web.Controllers
{
    public class CardController : Controller
    {
        private readonly IMediator _med;
        private readonly UserIdentity _userIdentity;

        public CardController(IMediator med, UserIdentity userIdentity)
        {
            _med = med;
            _userIdentity = userIdentity;
        }


        public async Task<IActionResult> Index(long id, string backUrl)
        {
            var resp = await _med.Send(new GetCardQuery(id));

            return View(new CardIndexModel(
                new CardModel(
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
                    ),
                backUrl));
        }

        public async Task<IActionResult> SelectNewDivider(long id)
        {
            var resp = await _med.Send(new GetCardQuery(id));

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

        public async Task<IActionResult> Create(string address, string passageAddress, long bibleId, string action)
        {
            if (action == "search" || action == "changeBible")
            {
                var resp = await _med.Send(new GetBiblePassageQuery(_userIdentity.UserId, action == "search" ? address : passageAddress, bibleId));

                return View(new CreateCardModel
                {
                    LastAction = action,
                    IsValidAddress = resp.IsValidAddress,
                    IsCardAlreadyCreated = resp.IsAlreadyCreated,
                    Passage = resp.Passage == null ? null : new PassageModel
                    {
                        BibleId = resp.Passage.BibleId,
                        BookName = resp.Passage.BookName,
                        FromChapterNumber = resp.Passage.FromChapterNumber,
                        FromVerseNumber = resp.Passage.FromVerseNumber,
                        ToChapterNumber = resp.Passage.ToChapterNumber,
                        ToVerseNumber = resp.Passage.ToVerseNumber,
                        Verses = resp.Passage.Verses
                    },
                    Bibles = await GetAvailableBibleTranslations()
                });
            }
            else if(action == "create")
            {
                await _med.Send(new CreateCardCommand(_userIdentity.UserId, bibleId, passageAddress));
                return View(new CreateCardModel { LastAction = action });
            }

            return View(new CreateCardModel());
        }

        public async Task<IActionResult> Move(long id, Divider toDivider)
        {
            await _med.Send(new MoveCardCommand(id, toDivider));

            var backUrl = Url.Action("Index", "Set");

            if(toDivider == Divider.Queue)
                backUrl = Url.Action("Queue", "Set");
            else if(Divider.Day1 <= toDivider && toDivider <= Divider.Day31)
                backUrl = Url.Action("Day", "Set", new { Id = (int)toDivider - (int)Divider.Day1 + 1 });


            return RedirectToAction("Index", new { id, backUrl = backUrl });
        }

        public async Task<IActionResult> Delete(long id, string returnUrl)
        {
            await _med.Send(new DeleteCardCommand(id));
            if(string.IsNullOrWhiteSpace(returnUrl))
                return RedirectToAction("Index", "Home");
            else
                return Redirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Reorder([FromBody] ReorderCardRequestModel model)
        {
            try
            {
                await _med.Send(new ReorderCardCommand(model.GetDividerEnum(), _userIdentity.UserId, model.CardId, model.NewOrder));
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<List<SelectListItem>> GetAvailableBibleTranslations()
            => (await _med.Send(new GetBibleTranslationsQuery())).Translations.Select(t => new SelectListItem($"{t.LongName} ({t.ShortName})", t.Id.ToString())).ToList();
    }
}

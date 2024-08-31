using BitPantry.Iota.Application.CRQS.Bible.Query;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.CRQS.Card.Query;
using BitPantry.Iota.Web.Models;
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


        public async Task<IActionResult> Index(long id)
        {
            var resp = await _med.Send(new GetCardQuery(id));

            return View(new CardModel
            {
                Id = resp.Id,
                AddedOn = resp.AddedOn,
                LastMovedOn = resp.LastMovedOn,
                Divider = resp.Divider,
                Verses = resp.Verses
            });
        }

        public async Task<IActionResult> Create(string address, string passageAddress, long bibleId, string action)
        {
            if (action == "search" || action == "changeBible")
            {
                var resp = await _med.Send(new GetBiblePassageQuery(action == "search" ? address : passageAddress, bibleId));

                return View(new CreateCardModel
                {
                    LastAction = action,

                    IsValidAddress = resp.IsValidAddress,

                    BibleId = resp.BibleId,
                    BookName = resp.BookName,
                    FromChapterNumber = resp.FromChapterNumber,
                    FromVerseNumber = resp.FromVerseNumber,
                    ToChapterNumber = resp.ToChapterNumber,
                    ToVerseNumber = resp.ToVerseNumber,

                    Verses = resp.Verses,

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

        [HttpPost]
        public async Task<IActionResult> Reorder([FromBody] ReorderCardRequestModel model)
        {
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                Console.WriteLine(body); // Log the raw request body
            }


            try
            {
                await _med.Send(new ReorderCardCommand(model.CardId, model.NewOrder));
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

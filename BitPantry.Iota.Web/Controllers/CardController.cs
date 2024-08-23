using BitPantry.Iota.Application.CRQS.Bible.Query;
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

        public CardController(IMediator med)
        {
            _med = med;
        }

        public async Task<IActionResult> Create(string address, long bibleId)
        {
            if (string.IsNullOrEmpty(address))
                return View(new BiblePassage());

            var resp = await _med.Send(new GetBiblePassageQuery(address, bibleId));

            return View(new BiblePassage
            {
                QueryAddress = address,
                IsValidAddress = resp.IsValidAddress,
                BibleId = resp.BibleId,
                BookName = resp.BookName,
                ChapterNumber = resp.ChapterNumber,
                VerseNumberFrom = resp.VerseNumberFrom,
                VerseNumberTo = resp.VerseNumberTo,
                Verses = resp.Verses,
                Bibles = await GetAvailableBibleTranslations()
            });
        }

        private async Task<List<SelectListItem>> GetAvailableBibleTranslations()
            => (await _med.Send(new GetBibleTranslationsQuery())).Translations.Select(t => new SelectListItem($"{t.LongName} ({t.ShortName})", t.Id.ToString())).ToList();






    }
}

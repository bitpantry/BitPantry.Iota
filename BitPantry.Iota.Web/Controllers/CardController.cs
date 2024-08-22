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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new CreateCardModel(await GetBibleTranslationsSelectList()));
        }

        [HttpGet]
        public async Task<IActionResult> GetBiblePassage(long bibleId, string passage)
        {
            var resp = await _med.Send(new GetBiblePassageQuery(bibleId, passage));
    
            return PartialView("_BiblePassage", new BiblePassage
            {
                IsValid = resp.IsValid,
                BibleId = resp.BibleId,
                TranslationShortName = resp.TranslationShortName,
                TranslationLongName = resp.TranslationLongName,
                BookNumber = resp.BookNumber,
                BookName = resp.BookName,
                ChapterNumber = resp.ChapterNumber,
                VerseNumberFrom = resp.VerseNumberFrom,
                VerseNumberTo = resp.VerseNumberTo,
                Verses = resp.Verses
            });
        }


        private async Task<List<SelectListItem>> GetBibleTranslationsSelectList()
            => (await _med.Send(new GetBibleTranslationsQuery())).Translations.Select(t => new SelectListItem(t.ShortName, t.Id.ToString())).ToList();
        
    }
}

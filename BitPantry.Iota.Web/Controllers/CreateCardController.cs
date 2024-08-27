using BitPantry.Iota.Application.CRQS.Bible.Query;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace BitPantry.Iota.Web.Controllers
{
    public class CreateCardController : Controller
    {
        private readonly IMediator _med;
        private readonly UserIdentity _userIdentity;

        public CreateCardController(IMediator med, UserIdentity userIdentity)
        {
            _med = med;
            _userIdentity = userIdentity;
        }

        [HttpGet]
        public IActionResult Index()
            => View(new CreateCardModel());

        public async Task<IActionResult> Search(string address, long bibleId)
        {
            var resp = await _med.Send(new GetBiblePassageQuery(address, bibleId));

            return View(nameof(Index), new CreateCardModel
            {
                IsHttpPost = true,

                IsValidAddress = resp.IsValidAddress,

                BibleId = resp.BibleId,
                BookName = resp.BookName,
                ChapterNumber = resp.ChapterNumber,
                FromVerseNumber = resp.FromVerseNumber,
                ToVerseNumber = resp.ToVerseNumber,

                Verses = resp.Verses,

                Bibles = await GetAvailableBibleTranslations()
            });
        }

        public async Task<IActionResult> Create(string address, long bibleId)
        {
            await _med.Send(new CreateCardCommand(_userIdentity.UserId, bibleId, address));
            return View(nameof(Index), new CreateCardModel { IsSuccessfulCreate = true });
        }

        private async Task<List<SelectListItem>> GetAvailableBibleTranslations()
            => (await _med.Send(new GetBibleTranslationsQuery())).Translations.Select(t => new SelectListItem($"{t.LongName} ({t.ShortName})", t.Id.ToString())).ToList();
    }
}

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

        [HttpPost]
        public async Task<IActionResult> Create(CreateCardModel model)
        {
            if(!ModelState.IsValid)
                return View(model);

            return View(new CreateCardModel(await GetBibleTranslationsSelectList()));
        }

        private async Task<List<SelectListItem>> GetBibleTranslationsSelectList()
            => (await _med.Send(new GetBibleTranslationsQuery())).Translations.Select(t => new SelectListItem(t.ShortName, t.Id.ToString())).ToList();
        
    }
}

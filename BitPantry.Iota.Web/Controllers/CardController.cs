using BitPantry.Iota.Application.CRQS.Bible.Query;
using BitPantry.Iota.Application.CRQS.Card.Command;
using BitPantry.Iota.Application.CRQS.Card.Query;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Components.RenderTree;
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

        [Route("card/{id:long}")]
        public async Task<IActionResult> Maintenance(long id)
        {
            var card = await _med.Send(new GetCardQuery(id));

            if (card == null)
                return NotFound();

            return View(card.ToModel());
        }

        public async Task<IActionResult> SelectNewTab(long id)
        {
            var resp = await _med.Send(new GetCardQuery(id) { IncludePassage = false });
            return View(resp.ToModel());
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
                    Passage = resp.Passage?.ToModel(),
                    Bibles = await GetAvailableBibleTranslations()
                });
            }
            else if(action == "create")
            {
                var resp = await _med.Send(new CreateCardCommand(_userIdentity.UserId, bibleId, passageAddress));
                return View(new CreateCardModel { LastAction = action, CardCreatedInTab = resp.Tab });
            }

            return View(new CreateCardModel());
        }

        public async Task<IActionResult> Move(long id, Tab toTab)
        {
            await _med.Send(new MoveCardCommand(id, toTab));
            return Route.RedirectTo<TabsController>(c => c.Tabs(toTab));      
        }

        public async Task<IActionResult> Delete(long id)
        {
            await _med.Send(new DeleteCardCommand(id));

            var referer = Request.Headers.Referer.ToString();
            return !string.IsNullOrEmpty(referer) ? Redirect(referer) : Route.RedirectTo<HomeController>(c => c.Index());
        }

        [HttpPost]
        public async Task<IActionResult> Reorder([FromBody] ReorderCardRequestModel model)
        {
            try
            {
                await _med.Send(new ReorderCardCommand(model.GetTabEnum(), _userIdentity.UserId, model.CardId, model.NewOrder));
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

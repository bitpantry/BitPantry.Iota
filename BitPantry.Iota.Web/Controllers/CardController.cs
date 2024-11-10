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
using System;
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

        public async Task<IActionResult> Create(string address, long bibleId)
        {
            var biblesResp = await _med.Send(new GetBibleTranslationsQuery());

            if(!string.IsNullOrEmpty(address))
            {
                var passageResp = await _med.Send(new GetBiblePassageQuery(_userIdentity.UserId, address, bibleId));

                return View(nameof(Create), new CreateCardModel
                {
                    Bibles = biblesResp.Select(b => b.ToModel()).ToList(),
                    BibleId = bibleId,
                    IsValidAddress = passageResp.IsValidAddress,
                    IsCardAlreadyCreated = passageResp.IsAlreadyCreated,
                    AddressQuery = address,
                    Passage = passageResp.Passage?.ToModel()
                });

            }

            return View(nameof(Create), new CreateCardModel
            {
                Bibles = biblesResp.Select(b => b.ToModel()).ToList(),
                BibleId = biblesResp.First().Id
            });
        }

        public async Task<IActionResult> New(string address, long bibleId)
        {
            var resp = await _med.Send(new CreateCardCommand(_userIdentity.UserId, bibleId, address));

            if (!resp.IsValidAddress || resp.isAlreadyCreated)
            {
                return await Create(address, bibleId);
            }
            else
            {
                var biblesResp = await _med.Send(new GetBibleTranslationsQuery());

                return View(nameof(Create), new CreateCardModel
                {
                    Bibles = biblesResp.Select(_ => _.ToModel()).ToList(),
                    BibleId = biblesResp.First().Id,
                    CreatedAddress = resp.Address,
                    CreatedCardId = resp.CardId,
                    CreatedToTab = resp.Tab,
                });
            }
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

    }
}

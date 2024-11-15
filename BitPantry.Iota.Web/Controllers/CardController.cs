using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Application.Parsers;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace BitPantry.Iota.Web.Controllers
{
    public class CardController : Controller
    {
        private readonly CardService _cardSvc;
        private readonly UserIdentity _userIdentity;
        private readonly BibleService _bibleSvc;

        public CardController(CardService cardSvc, BibleService bibleSvc, UserIdentity userIdentity)
        {
            _cardSvc = cardSvc;
            _userIdentity = userIdentity;
            _bibleSvc = bibleSvc;
        }

        [Route("card/{id:long}")]
        public async Task<IActionResult> Maintenance(long id)
        {
            var card = await _cardSvc.GetCard(id, HttpContext.RequestAborted);

            if (card == null)
                return NotFound();

            return View(card.ToModel());
        }

        public async Task<IActionResult> SelectNewTab(long id)
        {
            var resp = await _cardSvc.GetCardWithoutPassage(id, HttpContext.RequestAborted);
            return View(resp.ToModel());
        }

        public async Task<IActionResult> Create(string address, long bibleId)
        {
            var biblesResp = await _bibleSvc.GetBibleTranslations(HttpContext.RequestAborted);

            if(!string.IsNullOrEmpty(address))
            {
                var resp = await _bibleSvc.GetBiblePassage(bibleId, address, HttpContext.RequestAborted);

                var isAlreadyCreated = resp.ParsingException == null && await _cardSvc.DoesCardAlreadyExist(_userIdentity.UserId, bibleId, address, HttpContext.RequestAborted);

                return View(nameof(Create), new CreateCardModel
                {
                    Bibles = biblesResp.Select(b => b.ToModel()).ToList(),
                    BibleId = bibleId,
                    IsValidAddress = resp.ParsingException == null,
                    IsCardAlreadyCreated = isAlreadyCreated,
                    AddressQuery = address,
                    Passage = resp.Passage?.ToModel()
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
            var resp = await _cardSvc.CreateCard(_userIdentity.UserId, bibleId, address, HttpContext.RequestAborted);

            if (resp.Result != Application.CreateCardResponseResult.Ok)
            {
                return await Create(address, bibleId);
            }
            else
            {
                var biblesResp = await _bibleSvc.GetBibleTranslations(HttpContext.RequestAborted);

                return View(nameof(Create), new CreateCardModel
                {
                    Bibles = biblesResp.Select(_ => _.ToModel()).ToList(),
                    BibleId = biblesResp.First().Id,
                    CreatedAddress = resp.Card.Address,
                    CreatedCardId = resp.Card.Id,
                    CreatedToTab = resp.Card.Tab,
                });
            }
        }

        public async Task<IActionResult> Move(long id, Tab toTab)
        {
            await _cardSvc.MoveCard(id, toTab, HttpContext.RequestAborted);
            return Route.RedirectTo<TabsController>(c => c.Tabs(toTab));      
        }

        public async Task<IActionResult> Delete(long id)
        {
            var card = await _cardSvc.GetCard(id, HttpContext.RequestAborted);
            await _cardSvc.DeleteCard(id, HttpContext.RequestAborted);

            var referer = Request.Headers.Referer.ToString();
            return Route.RedirectTo<TabsController>(c => c.Tabs(card.Tab));
        }

        [HttpPost]
        public async Task<IActionResult> Reorder([FromBody] ReorderCardRequestModel model)
        {
            try
            {
                await _cardSvc.ReorderCard(_userIdentity.UserId, model.GetTabEnum(), model.CardId, model.NewOrder, HttpContext.RequestAborted);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}

using BitPantry.Tabs.Application;
using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Application.Parsers;
using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace BitPantry.Tabs.Web.Controllers
{
    public class CardController : Controller
    {
        private readonly CardService _cardSvc;
        private readonly IWorkflowService _workflowSvc;
        private readonly CurrentUser _currentUser;
        private readonly BibleService _bibleSvc;
        private readonly TabsService _tabSvc;

        public CardController(CardService cardSvc, IWorkflowService workflowSvc, BibleService bibleSvc, TabsService tabSvc, CurrentUser currentUser)
        {
            _cardSvc = cardSvc;
            _workflowSvc = workflowSvc;
            _currentUser = currentUser;
            _bibleSvc = bibleSvc;
            _tabSvc = tabSvc;
        }

        [Route("card/{id:long}")]
        public async Task<IActionResult> Maintenance(long id)
        {
            var card = await _cardSvc.GetCard(id, HttpContext.RequestAborted);

            if (card == null)
                return NotFound();

            return View(card.ToMaintenanceModel(_currentUser.WorkflowType));
        }

        public async Task<IActionResult> New(string address, long bibleId)
        {
            var biblesResp = await _bibleSvc.GetBibleTranslations(HttpContext.RequestAborted);

            if(!string.IsNullOrEmpty(address))
            {
                var resp = await _bibleSvc.GetBiblePassage(bibleId, address, HttpContext.RequestAborted);

                var isAlreadyCreated = resp.ParsingException == null && await _cardSvc.DoesCardAlreadyExistForUser(_currentUser.UserId, bibleId, address, HttpContext.RequestAborted);

                return View(nameof(New), new NewCardModel
                {
                    Bibles = biblesResp.Select(b => b.ToModel()).ToList(),
                    BibleId = bibleId,
                    IsValidAddress = resp.ParsingException == null,
                    IsCardAlreadyCreated = isAlreadyCreated,
                    AddressQuery = address,
                    Passage = resp.Passage?.ToModel(),
                    WorkflowType = _currentUser.WorkflowType
                });

            }

            return View(nameof(New), new NewCardModel
            {
                Bibles = biblesResp.Select(b => b.ToModel()).ToList(),
                BibleId = biblesResp.First().Id
            });
        }

        public async Task<IActionResult> Move(long id, Tab toTab)
        {
            var card = await _cardSvc.GetCard(id);

            await _workflowSvc.MoveCard(id, toTab, false, HttpContext.RequestAborted);
          
            var maintReferrer = Request.GetCardMaintenanceReferrer();

            if (maintReferrer == CardMaintenanceReferrer.CardMaintenance)
                return Route.RedirectTo<CardController>(c => c.Maintenance(id));
            else
                return Route.RedirectTo<CollectionController>(c => c.Index(card.Tab));
        }

        public async Task<IActionResult> Create(string address, long bibleId)
        {
            var resp = _currentUser.WorkflowType == WorkflowType.Basic
                ? await _cardSvc.CreateCard(_currentUser.UserId, bibleId, address, HttpContext.RequestAborted)
                : await _cardSvc.CreateCard(_currentUser.UserId, bibleId, address, (Tab)Enum.Parse(typeof(Tab), Request.Form["addToTab"]), HttpContext.RequestAborted);            

            if (resp.Result != Application.CreateCardResponseResult.Ok)
            {
                return await New(address, bibleId);
            }
            else
            {
                var biblesResp = await _bibleSvc.GetBibleTranslations(HttpContext.RequestAborted);

                return View(nameof(New), new NewCardModel
                {
                    Bibles = biblesResp.Select(_ => _.ToModel()).ToList(),
                    BibleId = biblesResp.First().Id,
                    CreatedAddress = resp.Card.Address,
                    CreatedCardId = resp.Card.Id,
                    CreatedToTab = resp.Card.Tab,
                });
            }
        }

        public async Task<IActionResult> Delete(long id)
        {
            var card = await _cardSvc.GetCard(id, HttpContext.RequestAborted);
            await _workflowSvc.DeleteCard(id, HttpContext.RequestAborted);

            return Route.RedirectTo<CollectionController>(c => c.Index(card.Tab));
        }

        public async Task<IActionResult> SendBackToQueue(long id)
        {
            await _workflowSvc.MoveCard(id, Tab.Queue, false, HttpContext.RequestAborted);

            var card = await _cardSvc.GetCard(id, HttpContext.RequestAborted);
            var cardCountInQueue = await _tabSvc.GetCardCountForTab(_currentUser.UserId, Tab.Queue, HttpContext.RequestAborted);

            var maintReferrer = Request.GetCardMaintenanceReferrer();

            if (maintReferrer == CardMaintenanceReferrer.CardMaintenance)
                return Route.RedirectTo<CardController>(c => c.Maintenance(id));
            else
                return Route.RedirectTo<CollectionController>(c => c.Index(card.Tab));
        }

        public async Task<IActionResult> StartNow(long id)
        {
            if (_currentUser.WorkflowType != WorkflowType.Basic)
                return Forbid();

            await _workflowSvc.StartQueueCard(id, HttpContext.RequestAborted);
            return Route.RedirectTo<ReviewController>(c => c.Index());
        }

        public async Task<IActionResult> MoveToDailyTab(long id)
        {
            if (_currentUser.WorkflowType != WorkflowType.Advanced)
                return Forbid();

            await _workflowSvc.MoveCard(id, Tab.Daily, false, HttpContext.RequestAborted);

            var cardCountInDaily = await _tabSvc.GetCardCountForTab(_currentUser.UserId, Tab.Daily, HttpContext.RequestAborted);

            var maintReferrer = Request.GetCardMaintenanceReferrer();
            
            if(maintReferrer == CardMaintenanceReferrer.CardMaintenance)
                return Route.RedirectTo<CardController>(c => c.Maintenance(id));
            else 
                return Route.RedirectTo<CollectionController>(c => c.Index(Tab.Queue));
        }

        [HttpPost]
        public async Task<IActionResult> Reorder([FromBody] ReorderCardRequestModel model)
        {
            try
            {
                await _cardSvc.ReorderCard(_currentUser.UserId, model.GetTabEnum(), model.CardId, model.NewOrder, HttpContext.RequestAborted);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}


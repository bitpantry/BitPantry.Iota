using BitPantry.Iota.Application.CRQS.Card.Query;
using BitPantry.Iota.Application.CRQS.Tabs.Query;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BitPantry.Iota.Web.Controllers
{
    public class TabsController : Controller
    {
        private IMediator _med;
        private UserIdentity _identity;

        public TabsController(IMediator med, UserIdentity identity) 
        {
            _med = med;
            _identity = identity;
        }

        [Route("tabs")]
        public async Task<IActionResult> Index()
            => await Tabs(Common.Tab.Queue);

        [Route("tab/{tab:enum}")]
        public async Task<IActionResult> Tabs(Tab tab)
        {
            var counts = await _med.Send(new GetTabCardCountsQuery(_identity.UserId));

            var model = new TabsModel
            {
                ActiveTab = tab,
                WeekdaysWithData = counts.Keys.Where(d => d >= Common.Tab.Sunday && d <= Common.Tab.Saturday).ToArray(),
                DaysOfMonthWithData = counts.Keys.Where(d => d > Common.Tab.Saturday).ToArray()
            };

            //if(tab > Common.Tab.Saturday)
            //{
            //    var cards = await _med.Send(new GetCardsForTabQuery(_identity.UserId, tab));

            //    // todo - in this new model, all I need is the id and the passage address (card.text in view)
            //    // so, need to rewrite the get query to not be so complex and the model can be signficantly simplified

            //    // todo - move the GetDividerCardCountsQuery to Cards as rename to GetCardCountByDividerQuery
                
            //    if(cards.Any())
            //        model.SortableSet = cards
            //    new SortableSetModel(dayDivider.Humanize(), dayDivider, resp.Select(i => new SetCardModel(i.CardId, i.Address, i.Order)).ToList(), "Month", Url.Action("Day", "Set", new { Id = id })));
            //}

            return View(nameof(Tabs), model);
        }
    }
}

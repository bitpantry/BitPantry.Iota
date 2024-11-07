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
            => await Tabs(Tab.Queue);

        [Route("tabs/{tab:enum}")]
        public async Task<IActionResult> Tabs(Tab tab)
        {
            var counts = await _med.Send(new GetTabCardCountsQuery(_identity.UserId));

            // find a tab with data to set as active

            if (tab == Tab.Sunday)
                tab = GetNextSubTabWithData([Tab.Sunday, Tab.Monday, Tab.Tuesday, Tab.Wednesday, Tab.Thursday, Tab.Friday, Tab.Saturday], counts);

            if (tab == Tab.Day1)
                tab = GetNextSubTabWithData(
                        Enum.GetValues(typeof(Tab))
                            .Cast<Tab>()
                            .Where(t => t >= Tab.Day1 && t <= Tab.Day31)
                            .ToArray(),
                        counts);

            // query cards

            var cards = await _med.Send(new GetCardsForTabQuery(_identity.UserId, tab));

            // build model

            var model = new TabsModel
            {
                ActiveTab = tab,
                WeekdaysWithData = counts.Keys.Where(d => d >= Common.Tab.Sunday && d <= Common.Tab.Saturday).ToArray(),
                DaysOfMonthWithData = counts.Keys.Where(d => d > Common.Tab.Saturday).ToArray(),
                Cards = cards.Select(c => c.ToModel()).ToList()
            };


            return View(nameof(Tabs), model);
        }

        private Tab GetNextSubTabWithData(Tab[] subTabs, Dictionary<Tab, int> tabsWithData)
        {
            foreach (var tab in subTabs)
            {
                if (tabsWithData.ContainsKey(tab))
                    return tab;
            }

            return subTabs.First();
        }
    }
}

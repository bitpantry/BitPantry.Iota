using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BitPantry.Iota.Web.Controllers
{
    public class CollectionController : Controller
    {
        private UserIdentity _identity;
        private TabsService _tabSvc;

        public CollectionController(UserIdentity identity, TabsService tabSvc) 
        {
            _identity = identity;
            _tabSvc = tabSvc;
        }

        public async Task<IActionResult> Index()
            => await Index(Tab.Queue);

        [Route("collection/{tab:enum}")]
        public async Task<IActionResult> Index(Tab tab)
        {
            var counts = await _tabSvc.GetCardCountByTab(_identity.UserId, HttpContext.RequestAborted);

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

            var cards = await _tabSvc.GetCardsForTab(_identity.UserId, tab, HttpContext.RequestAborted);

            // build model

            var model = new TabsModel
            {
                ActiveTab = tab,
                WeekdaysWithData = counts.Keys.Where(d => d >= Common.Tab.Sunday && d <= Common.Tab.Saturday).ToArray(),
                DaysOfMonthWithData = counts.Keys.Where(d => d > Common.Tab.Saturday).ToArray(),
                Cards = cards.Select(c => c.ToModel()).ToList()
            };


            return View(nameof(Index), model);
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

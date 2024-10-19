using BitPantry.Iota.Application.CRQS.Set.Query;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BitPantry.Iota.Web.Controllers
{
    public class SetController : Controller
    {
        private IMediator _med;
        private UserIdentity _userIdentity;

        public SetController(IMediator med, UserIdentity userIdentity)
        {
            _med = med;
            _userIdentity = userIdentity;
        }

        public async Task<IActionResult> Index()
        {
            var resp = await _med.Send(new GetSetSummaryQuery(_userIdentity.UserId));

            return View(new SetIndexModel { 
                    QueueCardCount = resp.QueueCardCount,
                    Daily = resp.Daily == null ? null : new SetCardModel(resp.Daily.CardId, resp.Daily.Address, resp.Daily.Order),
                    Odd = resp.Odd == null ? null : new SetCardModel(resp.Odd.CardId, resp.Odd.Address, resp.Odd.Order),
                    Even = resp.Even == null ? null : new SetCardModel(resp.Even.CardId, resp.Even.Address, resp.Even.Order),
                    Sunday = resp.Sunday == null ? null : new SetCardModel(resp.Sunday.CardId, resp.Sunday.Address, resp.Sunday.Order),
                    Monday = resp.Monday == null ? null : new SetCardModel(resp.Monday.CardId, resp.Monday.Address, resp.Monday.Order),
                    Tuesday = resp.Tuesday == null ? null : new SetCardModel(resp.Tuesday.CardId, resp.Tuesday.Address, resp.Tuesday.Order),
                    Wednesday = resp.Wednesday == null ? null : new SetCardModel(resp.Wednesday.CardId, resp.Wednesday.Address, resp.Wednesday.Order),
                    Thursday = resp.Thursday == null ? null : new SetCardModel(resp.Thursday.CardId, resp.Thursday.Address, resp.Thursday.Order),
                    Friday = resp.Friday == null ? null : new SetCardModel(resp.Friday.CardId, resp.Friday.Address, resp.Friday.Order),
                    Saturday = resp.Saturday == null ? null : new SetCardModel(resp.Saturday.CardId, resp.Saturday.Address, resp.Saturday.Order),
                    DaysOfTheMonthCardCount = resp.DaysOfTheMonthCardCount
                });
        }

        public async Task<IActionResult> Queue()
        {
            var resp = await _med.Send(new GetDividerSetQuery(_userIdentity.UserId, Divider.Queue));
            return View("SortableSet", new SortableSetModel("Queue", Divider.Queue, resp.Select(i => new SetCardModel(i.CardId, i.Address, i.Order)).ToList(), "Index", Url.Action("Queue", "Set")));
        }

        public async Task<IActionResult> Month()
        {
            var resp = await _med.Send(new GetDaysOfTheMonthSetQuery(_userIdentity.UserId));
            return View(resp);
        }

        public async Task<IActionResult> Day(int id)
        {
            var dayDivider = Divider.Day1 + id - 1;

            var resp = await _med.Send(new GetDividerSetQuery(_userIdentity.UserId, dayDivider));
            return View("SortableSet", new SortableSetModel(dayDivider.Humanize(), dayDivider, resp.Select(i => new SetCardModel(i.CardId, i.Address, i.Order)).ToList(), "Month", Url.Action("Day", "Set", new { Id = id })));
        }
    }
}

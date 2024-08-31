using BitPantry.Iota.Application.CRQS.Set.Query;
using BitPantry.Iota.Web.Models;
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
                    Odd = resp.Odd == null ? null : new SetCardModel { CardId = resp.Odd.CardId, Text = resp.Odd.Address },
                    Even = resp.Even == null ? null : new SetCardModel { CardId = resp.Even.CardId, Text = resp.Even.Address },
                    Sunday = resp.Sunday == null ? null : new SetCardModel { CardId = resp.Sunday.CardId, Text = resp.Sunday.Address },
                    Monday = resp.Monday == null ? null : new SetCardModel { CardId = resp.Monday.CardId, Text = resp.Monday.Address },
                    Tuesday = resp.Tuesday == null ? null : new SetCardModel { CardId = resp.Tuesday.CardId, Text = resp.Tuesday.Address },
                    Wednesday = resp.Wednesday == null ? null : new SetCardModel { CardId = resp.Wednesday.CardId, Text = resp.Wednesday.Address },
                    Thursday = resp.Thursday == null ? null : new SetCardModel { CardId = resp.Thursday.CardId, Text = resp.Thursday.Address },
                    Friday = resp.Friday == null ? null : new SetCardModel { CardId = resp.Friday.CardId, Text = resp.Friday.Address },
                    Saturday = resp.Saturday == null ? null : new SetCardModel { CardId = resp.Saturday.CardId, Text = resp.Saturday.Address },
                    DaysOfTheMonthCardCount = resp.DaysOfTheMonthCardCount
                });
        }

        public async Task<IActionResult> Queue()
        {
            var resp = await _med.Send(new GetQueueSetQuery(_userIdentity.UserId));
            return View(resp.Select(i => new SetCardModel { CardId = i.CardId, Text = i.Address, Order = i.Order }).ToList());
        }
    }
}

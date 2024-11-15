using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.CRQS.Review.Query
{
    public class GetReviewPathQueryHandler : IRequestHandler<GetReviewPathQuery, Dictionary<Tab, int>>
    {
        private EntityDataContext _dbCtx;

        public GetReviewPathQueryHandler(EntityDataContext dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<Dictionary<Tab, int>> Handle(GetReviewPathQuery request, CancellationToken cancellationToken)
        {
            var path = new Dictionary<Tab, int>();
            var currentTab = Tab.Queue;

            do
            {
                currentTab = GetNextReviewTab(currentTab, request.UserLocalTime);
                path.Add(currentTab, 0);

            } while (currentTab < Tab.Day1);

            // select the card count grouped by tab for the userId

            var cardCounts = await _dbCtx.Cards
                .Where(card => card.UserId == request.UserId)
                .GroupBy(card => card.Tab)
                .Select(group => new
                {
                    Tab = group.Key,
                    CardCount = group.Count()
                })
                .ToListAsync();

            foreach (var count in cardCounts)
            {
                if(path.ContainsKey(count.Tab))
                    path[count.Tab] = count.CardCount;
            }

            path = new Dictionary<Tab, int>(path.Where(p => p.Value > 0));

            return path;
        }

        private Tab GetNextReviewTab(Tab? lastTab, DateTime userLocalTime)
        {
            return lastTab switch
            {
                Tab.Queue => Tab.Daily,
                Tab.Daily => userLocalTime.Day % 2 == 0 ? Tab.Even : Tab.Odd,
                Tab.Odd or Tab.Even => userLocalTime.DayOfWeek switch
                {
                    DayOfWeek.Sunday => Tab.Sunday,
                    DayOfWeek.Monday => Tab.Monday,
                    DayOfWeek.Tuesday => Tab.Tuesday,
                    DayOfWeek.Wednesday => Tab.Wednesday,
                    DayOfWeek.Thursday => Tab.Thursday,
                    DayOfWeek.Friday => Tab.Friday,
                    DayOfWeek.Saturday => Tab.Saturday,
                    _ => throw new ArgumentOutOfRangeException("DateTime.Today.DayOfWeek", userLocalTime.DayOfWeek, "A tab is not defined for this day of the week")
                },
                Tab.Sunday or Tab.Monday or Tab.Tuesday or Tab.Wednesday or Tab.Thursday or Tab.Friday or Tab.Saturday => userLocalTime.Day + Tab.Saturday,
                _ => throw new ArgumentOutOfRangeException(nameof(lastTab), lastTab.Value, "No review path is defined for this tab")
            };
        }
    }

    public record GetReviewPathQuery(long UserId, DateTime UserLocalTime) : IRequest<Dictionary<Tab, int>> { }
}

using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Application.Service
{
    public class ReviewService
    {
        private readonly EntityDataContext _dbCtx;

        public ReviewService(EntityDataContext dbCtx) 
        {
            _dbCtx = dbCtx;
        }

        public async Task<ReviewPathDto> GetReviewPath(long userId, DateTime userLocalTime, CancellationToken cancellationToken)
        {
            var path = new Dictionary<Tab, int>();
            var currentTab = Tab.Queue;

            do
            {
                currentTab = GetNextReviewTab(currentTab, userLocalTime);
                path.Add(currentTab, 0);

            } while (currentTab < Tab.Day1);

            // select the card count grouped by tab for the userId

            var cardCounts = await _dbCtx.Cards
                .AsNoTracking()
                .Where(card => card.UserId == userId)
                .GroupBy(card => card.Tab)
                .Select(group => new
                {
                    Tab = group.Key,
                    CardCount = group.Count()
                })
                .ToListAsync(cancellationToken);

            foreach (var count in cardCounts)
            {
                if (path.ContainsKey(count.Tab))
                    path[count.Tab] = count.CardCount;
            };

            return new ReviewPathDto(
                userId,
                new Dictionary<Tab, int>(path.Where(p => p.Value > 0)));
        }

        private Tab GetNextReviewTab(Tab? lastTab, DateTime userLocalTime) => lastTab switch
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

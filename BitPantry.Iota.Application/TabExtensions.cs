using BitPantry.Iota.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    public static class TabExtensions
    {
        public static Tab GetNextReviewTab(this Tab tab, DateTime userLocalTime) => tab switch
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
            _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, "No review path is defined for this tab")
        };
    }
}

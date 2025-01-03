using BitPantry.Tabs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Test.Playwright
{
    public static class TabExtensions
    {
        public static DateTime GetValidReviewDateTime(this Tab tab, int year = 2024, int month = 1, int hour = 12, int minute = 0, int second = 0)
        {
            int day = 1;

            if (tab == Tab.Even)
                day = 2;

            if (tab >= Tab.Sunday && tab <= Tab.Saturday)
                day = 7 + tab - Tab.Sunday;

            if (tab >= Tab.Day1)
                day = tab - Tab.Day1 + 1;

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}

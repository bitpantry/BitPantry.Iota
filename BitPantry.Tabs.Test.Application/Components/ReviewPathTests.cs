using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Test.Application.Components
{
    public class ReviewPathTests
    {
        [Theory]
        [InlineData(Tab.Daily, Tab.Odd, Tab.Daily, Tab.Odd, Tab.Even)]
        [InlineData(Tab.Daily, null, Tab.Daily)]
        [InlineData(Tab.Daily, Tab.Even, Tab.Daily, Tab.Even, Tab.Day1)]
        [InlineData(Tab.Even, Tab.Monday, Tab.Daily, Tab.Even, Tab.Monday)]
        [InlineData(Tab.Day1, null, Tab.Daily, Tab.Even, Tab.Monday, Tab.Day1)]
        [InlineData(Tab.Odd, null)]
        public void GetNext_NextLocated(Tab fromTab, Tab? expectedNext, params Tab[] pathData)
        {
            Dictionary<Tab, int> dict = new Dictionary<Tab, int>();

            foreach (var tab in pathData)
                dict.Add(tab, 1);

            var path = new ReviewPathDto(1, dict);

            path.GetNextStep(fromTab).Should().Be(expectedNext);
        }

    }
}

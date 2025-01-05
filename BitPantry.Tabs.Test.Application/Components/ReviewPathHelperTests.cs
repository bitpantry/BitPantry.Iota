using BitPantry.Tabs.Application;
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
    public class ReviewPathHelperTests
    {
        [Fact]
        public void SingleCardTab_NextLocated()
        {
            var dict = new Dictionary<Tab, int>
            {
                {Tab.Daily, 1 },
                {Tab.Odd, 1 }
            };

            var helper = new ReviewPathHelper(dict);

            var nextStep = helper.GetNextStep(Tab.Daily, 1);

            nextStep.Value.Key.Should().Be(Tab.Odd);
            nextStep.Value.Value.Should().Be(1);
        }

        [Fact]
        public void MultiCardTab_NextLocated()
        {
            var dict = new Dictionary<Tab, int>
            {
                {Tab.Daily, 2 },
                {Tab.Odd, 1 }
            };

            var helper = new ReviewPathHelper(dict);

            var nextStep = helper.GetNextStep(Tab.Daily, 1);

            nextStep.Value.Key.Should().Be(Tab.Daily);
            nextStep.Value.Value.Should().Be(2);
        }

        [Fact]
        public void MultiCardMultiStep_NextLocated()
        {
            var dict = new Dictionary<Tab, int>
            {
                {Tab.Daily, 2 },
                {Tab.Odd, 1 }
            };

            var helper = new ReviewPathHelper(dict);

            var nextStep = helper.GetNextStep(Tab.Daily, 1);

            nextStep.Value.Key.Should().Be(Tab.Daily);
            nextStep.Value.Value.Should().Be(2);

            nextStep = helper.GetNextStep(Tab.Daily, 2);

            nextStep.Value.Key.Should().Be(Tab.Odd);
            nextStep.Value.Value.Should().Be(1);
        }

        [Fact]
        public void SkipTabs_NextLocated()
        {
            var dict = new Dictionary<Tab, int>
            {
                {Tab.Daily, 1 },
                {Tab.Sunday, 1 }
            };

            var helper = new ReviewPathHelper(dict);

            var nextStep = helper.GetNextStep(Tab.Daily, 1);

            nextStep.Value.Key.Should().Be(Tab.Sunday);
            nextStep.Value.Value.Should().Be(1);
        }


    }
}

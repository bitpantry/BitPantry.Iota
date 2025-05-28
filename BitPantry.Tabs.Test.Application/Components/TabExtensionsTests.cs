using BitPantry.Tabs.Application;
using System;
using BitPantry.Tabs.Common;
using FluentAssertions;
using Xunit;

namespace BitPantry.Tabs.Test.Application.Components;

public class TabExtensionsTests
{
    [Theory]
    [InlineData(Tab.Queue, DayOfWeek.Monday, Tab.Daily)]
    [InlineData(Tab.Daily, DayOfWeek.Tuesday, Tab.Even)]
    [InlineData(Tab.Odd, DayOfWeek.Sunday, Tab.Sunday)]
    public void GetNextReviewTab_ReturnsExpected(Tab start, DayOfWeek day, Tab expected)
    {
        var result = start.GetNextReviewTab(new DateTime(2024, 1, (int)day + 1));
        result.Should().Be(expected);
    }
}

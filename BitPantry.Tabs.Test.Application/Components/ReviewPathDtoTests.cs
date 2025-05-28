using BitPantry.Tabs.Application.DTO;
using System.Collections.Generic;
using BitPantry.Tabs.Common;
using FluentAssertions;
using Xunit;

namespace BitPantry.Tabs.Test.Application.Components;

public class ReviewPathDtoTests
{
    [Fact]
    public void CardsToReviewCount_SumsValues()
    {
        var dto = new ReviewPathDto(1, new Dictionary<Tab, int>
        {
            {Tab.Daily, 1},
            {Tab.Odd, 2}
        });

        dto.CardsToReviewCount.Should().Be(3);
    }
}

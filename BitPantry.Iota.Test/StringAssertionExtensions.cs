using BitPantry.Iota.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test
{
    public static class StringAssertionExtensions
    {
        public static AndConstraint<StringAssertions> BeIgnoreCase(this StringAssertions assertions, string expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(string.Equals(assertions.Subject, expected, StringComparison.OrdinalIgnoreCase))
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:string} to be {0}{reason}, but found {1}.", expected, assertions.Subject);

            return new AndConstraint<StringAssertions>(assertions);
        }
    }

}

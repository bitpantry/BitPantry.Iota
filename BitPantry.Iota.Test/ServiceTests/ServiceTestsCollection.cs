using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitPantry.Iota.Test.Fixtures;
using Xunit;

namespace BitPantry.Iota.Test.ServiceTests
{
    [CollectionDefinition("Services")]
    public class ServiceTestsCollection : ICollectionFixture<ApplicationEnvironmentCollectionFixture>
    {
    }
}

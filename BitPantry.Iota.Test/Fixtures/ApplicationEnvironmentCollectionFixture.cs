using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test.Fixtures
{
    public class ApplicationEnvironmentCollectionFixture : IDisposable
    {
        public long BibleId { get; }
        public ApplicationEnvironment Environment { get; }
        public ApplicationEnvironmentCollectionFixture()
        {
            Environment = ApplicationEnvironment.Create();
            BibleId = Environment.InstallBible().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Environment.Dispose();
        }
    }
}

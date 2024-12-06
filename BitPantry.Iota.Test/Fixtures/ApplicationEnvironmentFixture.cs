using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BitPantry.Iota.Test.Fixtures
{
    /// <summary>
    /// Using xUnit a new TestClass instance is created for each test function, this fixture ensures that the TestEnvironment is
    /// initialized only once per fixture.
    /// </summary>
    public class ApplicationEnvironmentFixture : IDisposable
    {
        private readonly object _lock = new();
        private ApplicationEnvironment _testEnv = null;

        public bool IsInitialized { get; private set; }

        public ApplicationEnvironmentFixture() { }

        public ApplicationEnvironment Initialize(Action<ApplicationEnvironmentOptions> createOptAction = null, Action<ApplicationEnvironment> initAction = null)
        {
            lock (_lock)
            {
                if (!IsInitialized)
                {
                    _testEnv = ApplicationEnvironment.Create(createOptAction);
                    initAction?.Invoke(_testEnv);
                    IsInitialized = true;
                }
            }

            return _testEnv;
        }

        public void Dispose()
        {
            lock (_lock)
                _testEnv?.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Test
{
    /// <summary>
    /// Using xUnit a new TestClass instance is created for each test function, this fixture ensures that the TestEnvironment is
    /// initialized only once per fixture.
    /// </summary>
    public class TestEnvironmentFixture : IDisposable
    {
        private readonly object _lock = new();
        private readonly TestEnvironment _testEnv;

        public bool IsInitialized { get; private set; }

        public TestEnvironmentFixture() 
        {
            _testEnv = TestEnvironment.Create();
        }

        public TestEnvironment Initialize(Action<TestEnvironment> initAction = null)
        {
            lock (_lock)
            {
                if (!IsInitialized)
                {
                    initAction?.Invoke(_testEnv);
                    IsInitialized = true;
                }
            }

            return _testEnv;
        }

        public void Dispose()
        {
            lock (_lock) 
                _testEnv.Dispose();
        }
    }
}

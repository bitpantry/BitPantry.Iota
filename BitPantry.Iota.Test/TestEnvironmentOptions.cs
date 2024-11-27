using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace BitPantry.Iota.Test
{
    public class TestEnvironmentOptions
    {
        public bool InstallTestData { get; private set; } = false;

        public TestEnvironmentOptions WithTestData()
        {
            InstallTestData = true;
            return this;
        }
        
    }
}

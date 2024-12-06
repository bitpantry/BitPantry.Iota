using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BitPantry.Iota.Test.Fixtures
{
    [CollectionDefinition("env")]
    public class AppEnvironmentFixture_Collection : ICollectionFixture<AppEnvironmentFixture> { }

    public class AppEnvironmentFixture : IAsyncLifetime, IDisposable, IHaveServiceProvider
    {
        public ApplicationEnvironment Environment { get; private set; }
        public long BibleId { get; private set; }
        public IServiceProvider ServiceProvider => Environment.ServiceProvider;

        public AppEnvironmentFixture() { }

        public void Dispose()
        {
            Environment?.Dispose();
        }

        public async Task InitializeAsync()
        {
            Environment = await ApplicationEnvironment.Create();
            BibleId = await Environment.InstallBible();
        }

        public Task DisposeAsync()
        {
            Environment?.Dispose();
            return Task.CompletedTask;
        }
    }
}

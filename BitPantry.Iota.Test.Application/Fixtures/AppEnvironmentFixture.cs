namespace BitPantry.Iota.Test.Application.Fixtures
{
    [CollectionDefinition("env")]
    public class AppEnvironmentFixture_Collection : ICollectionFixture<AppEnvironmentFixture> { }

    public class AppEnvironmentFixture : IDisposable, IHaveServiceProvider
    {
        public ApplicationEnvironment Environment { get; private set; }
        public long BibleId { get; private set; }
        public IServiceProvider ServiceProvider => Environment.ServiceProvider;

        public AppEnvironmentFixture() 
        { 
            Environment = ApplicationEnvironment.Create();
            BibleId = Environment.InstallBibles().GetAwaiter().GetResult();  
        }

        public void Dispose()
        {
            Environment?.Dispose();
        }
    }
}

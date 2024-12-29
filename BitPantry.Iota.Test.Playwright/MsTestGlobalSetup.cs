using BitPantry.Iota.Application;
using BitPantry.Iota.Data.Entity;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace BitPantry.Iota.Test.Playwright
{
    public static class Fixture
    {
        public static WebHostEnvironment Environment { get; private set; }
        public static long BibleId { get; private set; }

        public static void Initialize(WebHostEnvironment environment, long bibleId)
        {
            Environment = environment;
            BibleId = bibleId;
        }

        public static class BrowserOptions
        {
            public static BrowserNewContextOptions Default
                => new BrowserNewContextOptions()
                {
                    ColorScheme = ColorScheme.Light,
                    ViewportSize = new()
                    {
                        Width = 1920,
                        Height = 1080
                    }
                };
        }
    }

    [TestClass]
    public class MsTestGlobalSetup
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var env = WebHostEnvironment.Create();
            var bibleId = env.InstallBibles().GetAwaiter().GetResult();

            Fixture.Initialize(env, bibleId);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Fixture.Environment?.Dispose();
        }
    }
}

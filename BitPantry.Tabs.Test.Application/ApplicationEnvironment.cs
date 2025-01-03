using BitPantry.Tabs.Application;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Infrastructure;
using BitPantry.Tabs.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BitPantry.Tabs.Test.Application
{
    public class ApplicationEnvironment : IDisposable, IHaveServiceProvider
    {
        private readonly object _lock = new object();

        private bool _isDeployed = false;

        public string ContextId { get; } = Crypt.GenerateSecureRandomString(8);
        public IServiceProvider ServiceProvider { get; }

        private ApplicationEnvironment(ApplicationEnvironmentOptions options)
        {
            System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

            var config = TabsAppBootstrap.BuildTabsAppConfiguration("Test");

            var settings = new InfrastructureAppSettings(config, ContextId);

            var services = new ServiceCollection().AddCoreTabsServices<DefaultWorkflowServiceProvider>(settings);

            services.AddSingleton(settings);
            services.AddScoped<LocalDb>();

            services.AddLogging(cfg =>
            {
                cfg.AddConfiguration(config.GetSection("Logging"));
                cfg.AddSimpleConsole(opt =>
                {
                    opt.SingleLine = true;
                });
            });

            ServiceProvider = services.BuildServiceProvider();
        }

        public static ApplicationEnvironment Create(Action<ApplicationEnvironmentOptions> createOptAction = null)
        {
            var opt = new ApplicationEnvironmentOptions();

            createOptAction?.Invoke(opt);

            var env = new ApplicationEnvironment(opt);
            env.Deploy();

            return env;
        }

        private void Deploy()
        {
            lock (_lock)
            {
                if (_isDeployed)
                    return;

                using (var scope = ServiceProvider.CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<LocalDb>().Deploy(true).Wait();

                    var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                    dbCtx.Database.Migrate();
                }

                _isDeployed = true;
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (!_isDeployed)
                    return;

                using (var scope = ServiceProvider.CreateScope())
                    scope.ServiceProvider.GetRequiredService<LocalDb>().DropDatabase();

                _isDeployed = false;
            }
        }
    }
}

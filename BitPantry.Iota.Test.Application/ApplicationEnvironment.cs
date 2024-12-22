using BitPantry.Iota.Application;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure;
using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BitPantry.Iota.Test.Application
{
    public class ApplicationEnvironment : IDisposable, IHaveServiceProvider
    {
        private readonly object _lock = new object();

        private bool _isDeployed = false;
        private readonly AppSettings _appSettings;

        public string ContextId { get; } = Crypt.GenerateSecureRandomString(8);
        public IServiceProvider ServiceProvider { get; }

        private ApplicationEnvironment(ApplicationEnvironmentOptions options)
        {
            System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

            var config = IotaAppBootstrap.BuildIotaConfiguration("Test");

            _appSettings = new AppSettings(config, ContextId);

            var services = new ServiceCollection().AddCoreIotaServices<DefaultWorkflowServiceProvider>(_appSettings);

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

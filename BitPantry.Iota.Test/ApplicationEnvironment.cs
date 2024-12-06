using BitPantry.Iota.Application;
using BitPantry.Iota.Application.IoC;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Infrastructure.Settings;
using BitPantry.Iota.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static System.Formats.Asn1.AsnWriter;

namespace BitPantry.Iota.Test
{
    public class ApplicationEnvironment : IDisposable, IHaveServiceProvider
    {
        private ApplicationEnvironmentOptions _options;

        private readonly AppSettings _appSettings;
        private readonly LocalDb _localDb;

        public string ContextId { get; } = Crypt.GenerateSecureRandomString(8);
        public IServiceProvider ServiceProvider { get; }

        private ApplicationEnvironment(ApplicationEnvironmentOptions options)
        {
            System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

            _options = options;

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

            _localDb = ServiceProvider.GetRequiredService<LocalDb>();
        }

        public static async Task<ApplicationEnvironment> Create(Action<ApplicationEnvironmentOptions> createOptAction = null)
        {
            var opt = new ApplicationEnvironmentOptions();

            createOptAction?.Invoke(opt);

            var env = new ApplicationEnvironment(opt);
            await env.Deploy();

            return env;
        }

        private async Task Deploy()
        {
            await _localDb.Deploy(true);

            using (var scope = ServiceProvider.CreateScope())
            {
                var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                dbCtx.Database.Migrate();
            }
        }

        private void Cleanup()
        {
            _localDb.DropDatabase();
        }

        public void Dispose() => Cleanup();
    }
}

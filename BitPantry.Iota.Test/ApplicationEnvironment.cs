using BitPantry.Iota.Application.IoC;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Infrastructure.Settings;
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
    public class ApplicationEnvironment : IDisposable
    {
        private object _lock = new object();

        private ApplicationEnvironmentOptions _options;

        private readonly AppSettings _appSettings;
        private readonly ServiceProvider? _serviceProvider;
        private readonly LocalDb _localDb;

        public string ContextId { get; } = Crypt.GenerateSecureRandomString(8);
        public bool IsDeployed { get; private set; } = false;

        private ApplicationEnvironment(ApplicationEnvironmentOptions options)
        {
            _options = options;

            var config = new ConfigurationManager()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
                .Build();

            _appSettings = new AppSettings(config, ContextId);

            var services = new ServiceCollection();

            services.ConfigureInfrastructureServices(_appSettings, CachingStrategy.InMemory);
            services.ConfigureApplicationServices(GetWorkflowService);

            services.AddScoped<LocalDb>();

            services.AddLogging(cfg =>
            {
                cfg.AddConfiguration(config.GetSection("Logging"));
                cfg.AddSimpleConsole(opt =>
                {
                    opt.SingleLine = true;
                });
            });

            _serviceProvider = services.BuildServiceProvider();

            _localDb = _serviceProvider.GetRequiredService<LocalDb>();
        }

        private IWorkflowService GetWorkflowService(IServiceProvider provider)
        {
            switch (_options.WorkflowType) {
                case Common.WorkflowType.Basic:
                    return provider.GetRequiredService<BasicWorkflowService>();
                case Common.WorkflowType.Advanced:
                    return provider.GetRequiredService<AdvancedWorkflowService>();
                default:
                    throw new NotImplementedException($"No case for {_options.WorkflowType} is implemented.");
            }
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
                if (IsDeployed) return;

                _localDb.Deploy(true).Wait();

                using (var scope = CreateDependencyScope())
                {
                    var dbCtx = scope.ServiceProvider.GetRequiredService<EntityDataContext>();
                    dbCtx.Database.Migrate();
                }

                IsDeployed = true;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                if(IsDeployed)
                    Cleanup();
                Deploy();
            }
        }

        public IServiceScope CreateDependencyScope()
            => _serviceProvider.CreateScope();

        private void Cleanup()
        {
            lock (_lock)
            {
                if (!IsDeployed) return;

                _localDb.DropDatabase();
                _serviceProvider?.Dispose();

                IsDeployed = false;
            }
        }

        public void Dispose() => Cleanup();
    }
}

using BitPantry.Iota.Application.IoC;
using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Infrastructure.Settings;
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
    public class TestEnvironment : IDisposable
    {
        private object _lock = new object();

        private TestEnvironmentOptions _options;

        private readonly AppSettings _appSettings;
        private readonly ServiceProvider? _serviceProvider;
        private readonly LocalDb _localDb;
        private readonly TestDataService _testDataService;

        public string ContextId { get; } = Crypt.GenerateSecureRandomString(8);
        public bool IsDeployed { get; private set; } = false;


        private TestEnvironment(TestEnvironmentOptions options)
        {
            _options = options;

            var config = new ConfigurationManager()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _appSettings = new AppSettings(config, ContextId);

            var services = new ServiceCollection();

            services.ConfigureInfrastructureServices(_appSettings, CachingStrategy.InMemory);
            services.ConfigureApplicationServices();

            services.AddScoped<TestDataService>();
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
            _testDataService = _serviceProvider.GetRequiredService<TestDataService>();
        }

        private void Deploy()
        {
            lock (_lock)
            {
                if (IsDeployed) return;

                _localDb.Deploy(true).Wait();
                _testDataService.Install(_options.InstallTestData).Wait();

                IsDeployed = true;
            }
        }

        public static TestEnvironment Deploy(Action<TestEnvironmentOptions> deployOptionsAction = null)
        {
            var opt = new TestEnvironmentOptions();

            deployOptionsAction?.Invoke(opt);

            var env = new TestEnvironment(opt);
            env.Deploy();

            return env;
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

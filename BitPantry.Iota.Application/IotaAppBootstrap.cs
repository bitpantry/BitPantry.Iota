using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitPantry.Iota.Application.IoC;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace BitPantry.Iota.Application
{
    public static class IotaAppBootstrap
    {      
        public static IServiceCollection AddCoreIotaServices<TWorkflowServiceProvider>(this IServiceCollection services, InfrastructureAppSettings settings) where TWorkflowServiceProvider : class, IWorkflowServiceProvider
        {
            services.AddScoped<IWorkflowServiceProvider, TWorkflowServiceProvider>();

            services.ConfigureInfrastructureServices(settings, CachingStrategy.InMemory);
            services.ConfigureApplicationServices();

            return services;
        }

        public static IConfigurationRoot BuildIotaAppConfiguration(string environmentName)
            => new ConfigurationManager().ConfigureForIotaApp(environmentName).Build();

        public static IConfigurationBuilder ConfigureForIotaApp(this IConfigurationBuilder mgr, string environmentName)
        {
            mgr.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<BookName>()
                .AddEnvironmentVariables("IOTA_");

            var config = mgr.Build();
            var settings = new InfrastructureAppSettings(config);

            if (!string.IsNullOrEmpty(settings.ConnectionStrings.AzureAppConfiguration))
            {
                mgr.AddAzureAppConfiguration(opt =>
                {
                    opt.Connect(settings.ConnectionStrings.AzureAppConfiguration)
                        .Select(KeyFilter.Any, environmentName);
                });
            }

            return mgr;
        }
    }
}

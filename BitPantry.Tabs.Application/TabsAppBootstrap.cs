using BitPantry.Tabs.Infrastructure.IoC;
using BitPantry.Tabs.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitPantry.Tabs.Application.IoC;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace BitPantry.Tabs.Application
{
    public static class TabsAppBootstrap
    {      
        public static IServiceCollection AddCoreTabsServices<TWorkflowServiceProvider>(this IServiceCollection services, InfrastructureAppSettings settings) where TWorkflowServiceProvider : class, IWorkflowServiceProvider
        {
            services.AddScoped<IWorkflowServiceProvider, TWorkflowServiceProvider>();

            services.ConfigureInfrastructureServices(settings, CachingStrategy.InMemory);
            services.ConfigureApplicationServices();

            return services;
        }

        public static IConfigurationRoot BuildTabsAppConfiguration(string environmentName)
            => new ConfigurationManager().ConfigureForTabsApp(environmentName).Build();

        public static IConfigurationBuilder ConfigureForTabsApp(this IConfigurationBuilder mgr, string environmentName)
        {
            mgr.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<BookName>()
                .AddEnvironmentVariables("TABS_");

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

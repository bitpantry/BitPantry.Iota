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

namespace BitPantry.Iota.Application
{
    public static class IotaAppBootstrap
    {
        public static IConfigurationRoot BuildIotaConfiguration(string environmentName)
            => new ConfigurationManager().ConfigureForIota(environmentName).Build();

        public static IConfigurationBuilder ConfigureForIota(this IConfigurationBuilder mgr, string environmentName)
        {
            mgr.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<BookName>();

            return mgr;
        }
        
        public static IServiceCollection AddCoreIotaServices<TWorkflowServiceProvider>(this IServiceCollection services, AppSettings settings) where TWorkflowServiceProvider : class, IWorkflowServiceProvider
        {
            services.AddScoped<IWorkflowServiceProvider, TWorkflowServiceProvider>();

            services.ConfigureInfrastructureServices(settings, CachingStrategy.InMemory);
            services.ConfigureApplicationServices();

            return services;
        }
    }
}

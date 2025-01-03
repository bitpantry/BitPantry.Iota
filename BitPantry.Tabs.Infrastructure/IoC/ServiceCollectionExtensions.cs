using BitPantry.Tabs.Data.Entity;
using BitPantry.Tabs.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BitPantry.Tabs.Infrastructure.Caching;
using Microsoft.Data.SqlClient;
using Thinktecture.EntityFrameworkCore;
using Thinktecture;

namespace BitPantry.Tabs.Infrastructure.IoC
{
    public enum CachingStrategy
    {
        InMemory
    }

    public static class ServiceCollectionExtensions
    {
        public static void ConfigureInfrastructureServices(
            this IServiceCollection services,
            InfrastructureAppSettings settings,
            CachingStrategy cachingStrategy)
        {
            // entity framework

            services.AddDbContextPool<EntityDataContext>(o =>
            {
                o.UseSqlServer(settings.ConnectionStrings.EntityDataContext, ssOpts => { ssOpts.AddWindowFunctionsSupport(); });
            });

            // configure caching

            switch (cachingStrategy)
            {
                case CachingStrategy.InMemory:
                    services.AddMemoryCache();
                    services.AddScoped<ICache, MemoryCache>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Caching strategy value, \"{cachingStrategy}\", is not defined for this switch");
            }

            services.AddTransient<CacheService>();
        }
    }
}

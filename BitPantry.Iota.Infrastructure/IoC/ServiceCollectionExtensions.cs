using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BitPantry.Iota.Infrastructure.Caching;

namespace BitPantry.Iota.Infrastructure.IoC
{
    public enum CachingStrategy
    {
        InMemory
    }

    public static class ServiceCollectionExtensions
    {
        public static void ConfigureInfrastructureServices(
            this IServiceCollection services,
            AppSettings settings,
            CachingStrategy cachingStrategy)
        {
            // database

            services.AddDbContextPool<EntityDataContext>(o => o.UseSqlServer(settings.ConnectionStrings.EntityDataContext));

            // extensions



            // repositories



            // services


            // components

            services.AddSingleton(settings);

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

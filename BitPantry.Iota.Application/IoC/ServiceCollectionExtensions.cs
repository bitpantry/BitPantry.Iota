using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Application.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BitPantry.Iota.Application.IoC
{

    public static class ServiceCollectionExtensions
    {
        public static void ConfigureApplicationServices(
            this IServiceCollection services)
        {
            // services

            services.AddScoped<BibleService>();
            services.AddScoped<CardService>();
            services.AddScoped<DataProtectionService>();
            services.AddScoped<IdentityService>();
            services.AddScoped<ReviewService>();
            services.AddScoped<TabsService>();

            // logic

            services.AddScoped<PassageLogic>();
            services.AddScoped<CardLogic>();

        }

    }


}

using BitPantry.Iota.Data.Entity;
using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BitPantry.Iota.Infrastructure.Caching;
using BitPantry.Iota.Infrastructure.IoC;

namespace BitPantry.Iota.Application.IoC
{

    public static class ServiceCollectionExtensions
    {
        public static void ConfigureApplicationServices(
            this IServiceCollection services,
            AppSettings settings)
        {

            // CRQS

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(System.Reflection.Assembly.GetExecutingAssembly()));

            // extensions

            

            // repositories

     

            // services


            // components



        }
    }
}

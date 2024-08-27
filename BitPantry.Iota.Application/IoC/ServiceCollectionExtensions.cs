using BitPantry.Iota.Application.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BitPantry.Iota.Application.IoC
{

    public static class ServiceCollectionExtensions
    {
        public static void ConfigureApplicationServices(
            this IServiceCollection services)
        {

            // CRQS

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(System.Reflection.Assembly.GetExecutingAssembly()));

            // extensions



            // repositories



            // services

            services.AddScoped<BibleService>();

            // components



        }
    }
}

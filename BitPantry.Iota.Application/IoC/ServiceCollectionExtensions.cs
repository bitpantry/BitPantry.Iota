using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Common;
using BitPantry.Iota.Infrastructure.Settings;
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



            // logic

            services.AddScoped<PassageLogic>();
            services.AddScoped<CardLogic>();

            // components



        }

    }


}

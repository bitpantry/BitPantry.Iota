using BitPantry.Iota.Application.Logic;
using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace BitPantry.Iota.Application.IoC
{

    public static class ServiceCollectionExtensions
    {
        public static void ConfigureApplicationServices(
            this IServiceCollection services,
            Func<IServiceProvider, IWorkflowService> workflowServiceProviderFunc)
        {
            // services

            services.AddScoped<BibleService>();
            services.AddScoped<CardService>();
            services.AddScoped<DataProtectionService>();
            services.AddScoped<IdentityService>();
            services.AddScoped<TabsService>();
            services.AddScoped<ArchiveService>();
            services.AddScoped<CardPromotionLogic>();
            services.AddScoped<UserService>();

            services.AddScoped<BasicWorkflowService>();
            services.AddScoped<AdvancedWorkflowService>();
            services.AddScoped(svc => workflowServiceProviderFunc(svc));

            // logic

            services.AddSingleton<CardPromotionLogic>();
            services.AddSingleton<PassageLogic>();

        }

    }


}

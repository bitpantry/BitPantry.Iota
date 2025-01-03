using BitPantry.Tabs.Application.Logic;
using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace BitPantry.Tabs.Application.IoC
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
            services.AddScoped<TabsService>();
            services.AddScoped<CardPromotionLogic>();
            services.AddScoped<UserService>();

            services.AddScoped<BasicWorkflowService>();
            services.AddScoped<AdvancedWorkflowService>();
            services.AddScoped<WorkflowServiceSwitch>();
            services.AddScoped(svc => svc.GetRequiredService<IWorkflowServiceProvider>().GetWorkflowService());

            // logic

            services.AddSingleton<CardPromotionLogic>();
            services.AddSingleton<PassageLogic>();

        }

    }


}

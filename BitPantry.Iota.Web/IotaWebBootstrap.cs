using BitPantry.Iota.Application.Service;
using BitPantry.Iota.Common;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Infrastructure.Settings;
using BitPantry.Iota.Web.IoC;
using BitPantry.Iota.Web.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using BitPantry.Iota.Application.IoC;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using BitPantry.Iota.Application;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Messaging.EventGrid.SystemEvents;
using BitPantry.Iota.Web.Settings;
using BitPantry.Iota.Web.Controllers;

namespace BitPantry.Iota.Web
{
    public static class IotaWebBootstrap
    {
        public static WebApplication BuildIotaWebApp(Action<WebApplicationBuilder> configBuilder = null, string[] args = null, string contextId = null)
        {
            var builder = WebApplication.CreateBuilder(args);

            // configure app settings

            builder.Configuration.ConfigureForIotaWeb(builder.Environment.EnvironmentName);
            var settings = new WebAppSettings(builder.Configuration, contextId);

            builder.Services.AddSingleton(settings);
            builder.Services.AddSingleton<InfrastructureAppSettings>(settings);

            // configure logging

            builder.Logging.ConfigureIotaWebLogging(builder.Environment.EnvironmentName, builder.Configuration);
                
            // configure services

            builder.Services.AddCoreIotaServices<WebIdentityWorkflowServiceProvider>(settings);
            builder.Services.AddIotaWebServices(settings);

            // pass the builder to the config builder action

            if (configBuilder != null)
                configBuilder(builder);

            // build the application

            return builder.Build().ConfigureIotaWebApplication(settings);
        }

        private static WebApplication ConfigureIotaWebApplication(this WebApplication app, WebAppSettings settings)
        {
            if (settings.EnableTestInfrastructure)
                app.Logger.LogWarning("Test infrastructure is enabled");

            // configure pipeline

            app.UseMiddleware<IotaLogEnricherMiddleware>();

            if(settings.EnableTestInfrastructure)
                app.UseMiddleware<TestInfrastructureMiddleware>();

            // Configure pipeline for environments

            if (app.Environment.IsProduction())
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                app.UseHttpsRedirection();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseMiddleware<AppStateCookieMiddleware>();

            if (settings.UseMiniProfiler)
                app.UseMiniProfiler();

            app.UseRouting();

            app.UseMiddleware<TimeZoneMiddleware>();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .RequireAuthorization();

            return app;
        }

        public static IServiceCollection AddIotaWebServices(this IServiceCollection services, WebAppSettings settings)
        {
            services.ConfigureWebServices(settings);
            services.ConfigureWebIdentityServices(settings);
            services.AddCoreIotaServices<WebIdentityWorkflowServiceProvider>(settings);

            if (settings.UseMiniProfiler)
                services.ConfigureMiniProfiler(settings);

            services.AddControllersWithViews();

            services.Configure<RouteOptions>(opt =>
            {
                opt.ConstraintMap.Add("enum", typeof(EnumRouteConstraint<Tab>));
            });

            return services;
        }

        public static void LogConfiguration(this WebApplication app)
        {
            var settings = app.Services.GetRequiredService<WebAppSettings>();
            var connStrBuilder = new SqlConnectionStringBuilder(settings.ConnectionStrings.EntityDataContext);
            app.Logger.LogInformation("BitPantry.Iota.Web created :: {Environment}, {DataSource} {Catalog}", app.Environment.EnvironmentName, connStrBuilder.DataSource, connStrBuilder.InitialCatalog);
        }

        public static IConfigurationRoot BuildIotaWebConfiguration(string environmentName)
            => new ConfigurationManager().ConfigureForIotaWeb(environmentName).Build();

        public static IConfigurationBuilder ConfigureForIotaWeb(this IConfigurationBuilder mgr, string environmentName)
        {
            mgr.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<HomeController>()
                .AddEnvironmentVariables("IOTA_");

            var config = mgr.Build();
            var settings = new WebAppSettings(config);

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

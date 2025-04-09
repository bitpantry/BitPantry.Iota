using BitPantry.Tabs.Application.Service;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Infrastructure.IoC;
using BitPantry.Tabs.Infrastructure.Settings;
using BitPantry.Tabs.Web.IoC;
using BitPantry.Tabs.Web.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using BitPantry.Tabs.Application.IoC;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using BitPantry.Tabs.Application;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Messaging.EventGrid.SystemEvents;
using BitPantry.Tabs.Web.Settings;
using BitPantry.Tabs.Web.Controllers;
using BitPantry.CommandLine.Remote.SignalR.Server.Configuration;
using BitPantry.Tabs.Web.Cli;
using BitPantry.CommandLine.Remote.SignalR.Server.Authentication;

namespace BitPantry.Tabs.Web
{
    public static class TabsWebBootstrap
    {
        public static WebApplication BuildTabsWebApp(Action<WebApplicationBuilder> configBuilder = null, string[] args = null, string contextId = null)
        {
            var builder = WebApplication.CreateBuilder(args);

            // configure app settings

            builder.Configuration.ConfigureForTabsWeb(builder.Environment.EnvironmentName);
            var settings = new WebAppSettings(builder.Configuration, contextId);

            builder.Services.AddSingleton(settings);
            builder.Services.AddSingleton<InfrastructureAppSettings>(settings);

            builder.Services.AddScoped<IApiKeyStore, CliKeyStore>();
            builder.Services.AddScoped<IRefreshTokenStore, CliRefreshTokenStore>();

            // add the command line hub

            builder.Services.AddCommandLineHub(opt =>
            {
                opt.AddJwtAuthentication<CliKeyStore, CliRefreshTokenStore>(settings.Identity.CliJwtSecret);
            });

            // configure logging

            builder.Logging.ConfigureTabsWebLogging(builder.Environment.EnvironmentName, builder.Configuration);
                
            // configure services

            builder.Services.AddCoreTabsServices<WebIdentityWorkflowServiceProvider>(settings);
            builder.Services.AddTabsWebServices(settings);

            // pass the builder to the config builder action

            if (configBuilder != null)
                configBuilder(builder);

            // build the application

            var app = builder.Build().ConfigureTabsWebApplication(settings);

            app.MapGet("/version", () => { return Results.Ok(new { version = AppVersionHelper.GetInformationalVersion() }); });

            return app;
        }

        private static WebApplication ConfigureTabsWebApplication(this WebApplication app, WebAppSettings settings)
        {
            if (settings.EnableTestInfrastructure)
                app.Logger.LogWarning("Test infrastructure is enabled");

            // configure pipeline

            app.UseMiddleware<TabsLogEnricherMiddleware>();

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

            app.ConfigureCommandLineHub();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .RequireAuthorization();

            return app;
        }

        public static IServiceCollection AddTabsWebServices(this IServiceCollection services, WebAppSettings settings)
        {
            services.ConfigureWebServices(settings);
            services.ConfigureWebIdentityServices(settings);
            services.AddCoreTabsServices<WebIdentityWorkflowServiceProvider>(settings);

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
            app.Logger.LogInformation("BitPantry.Tabs.Web created :: {Environment}, {DataSource} {Catalog}", app.Environment.EnvironmentName, connStrBuilder.DataSource, connStrBuilder.InitialCatalog);
        }

        public static IConfigurationRoot BuildTabsWebConfiguration(string environmentName)
            => new ConfigurationManager().ConfigureForTabsWeb(environmentName).Build();

        public static IConfigurationBuilder ConfigureForTabsWeb(this IConfigurationBuilder mgr, string environmentName)
        {
            mgr.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<HomeController>()
                .AddEnvironmentVariables("TABS_");

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

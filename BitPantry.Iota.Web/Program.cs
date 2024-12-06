using BitPantry.Iota.Infrastructure.Settings;
using BitPantry.Iota.Web.IoC;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Application.IoC;
using BitPantry.Iota.Web.Logging;
using BitPantry.Iota.Common;
using Microsoft.Data.SqlClient;
using System.Drawing.Text;
using BitPantry.Iota.Application.Service;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace BitPantry.Iota.Web
{
    public class Program
    {
        private static AppSettings _settings = null;

        public static void Main(string[] args)
        {
            CreateApp(CreateBuilder(args)).Run();
        }

        public static WebApplicationBuilder CreateBuilder(string[] args, string dataContextId = null)
        {
            var builder = WebApplication.CreateBuilder(args);

            // configure app settings

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();

            // configure logging

            builder.Logging.ConfigureIotaLogging(builder.Environment.EnvironmentName, builder.Configuration);

            // configure services

            _settings = new AppSettings(builder.Configuration, dataContextId);

            builder.Services.ConfigureWebServices(_settings);
            builder.Services.ConfigureIdentityServices(_settings);
            builder.Services.ConfigureInfrastructureServices(_settings, CachingStrategy.InMemory);
            builder.Services.ConfigureApplicationServices(GetWorkflowService);

            if (_settings.UseMiniProfiler)
                builder.Services.ConfigureMiniProfiler(_settings);

            builder.Services.AddControllersWithViews();

            builder.Services.Configure<RouteOptions>(opt =>
            {
                opt.ConstraintMap.Add("enum", typeof(EnumRouteConstraint<Tab>));
            });

            return builder;
        }

        public static WebApplication CreateApp(WebApplicationBuilder builder)
        {
            var app = builder.Build();

            var connStrBuilder = new SqlConnectionStringBuilder(_settings.ConnectionStrings.EntityDataContext);

            app.Logger.LogInformation("Starting BitPantry.Iota.Web :: {Environment}, {DataSource} {Catalog}", builder.Environment.EnvironmentName, connStrBuilder.DataSource, connStrBuilder.InitialCatalog);

            if (_settings.EnableTestInfrastructure)
                app.Logger.LogWarning("Test infrastructure is enabled");

            app.UseMiddleware<IotaLogEnricherMiddleware>();
            app.UseMiddleware<TimeZoneMiddleware>();

            // Configure the HTTP request pipeline.

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

            if (_settings.UseMiniProfiler)
                app.UseMiniProfiler();

            app.UseRouting();

            app.UseAuthorization();

            app.MapGet("/tst", () => "Hello!");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .RequireAuthorization();

            return app;
        }

        private static IWorkflowService GetWorkflowService(IServiceProvider svcProvider)
        {
            return svcProvider.GetRequiredService<BasicWorkflowService>();

            //var userIdentity = svcProvider.GetRequiredService<UserIdentity>();

            //if(userIdentity.UserId == 0)
            //    return svcProvider.GetRequiredService<BasicWorkflowService>();

            //var user = svcProvider.GetRequiredService<UserService>().GetUser(userIdentity.UserId).GetAwaiter().GetResult();

            //switch (user.WorkflowType)
            //{
            //    case WorkflowType.Basic:
            //        return svcProvider.GetRequiredService<BasicWorkflowService>();
            //    case WorkflowType.Advanced:
            //        return svcProvider.GetRequiredService<AdvancedWorkflowService>();
            //    default:
            //        throw new ArgumentOutOfRangeException($"WorkflowType {user.WorkflowType} is not defined for this sitch");
            //}
        }
    }
}

//https://www.youtube.com/watch?v=lxJutCKH1fs
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using BitPantry.Iota.Infrastructure.Settings;
using BitPantry.Iota.Web.IoC;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Application.IoC;
using BitPantry.Iota.Web.Logging;
using Microsoft.AspNetCore.Routing.Constraints;
using BitPantry.Iota.Common;
using Microsoft.Data.SqlClient;

namespace BitPantry.Iota.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // configure app settings

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // configure logging

            builder.Logging.ConfigureIotaLogging(builder.Environment.EnvironmentName, builder.Configuration);

            // configure services

            var settings = new AppSettings(builder.Configuration);

            builder.Services.ConfigureWebServices(settings);
            builder.Services.ConfigureIdentityServices(settings);
            builder.Services.ConfigureInfrastructureServices(settings, CachingStrategy.InMemory);
            builder.Services.ConfigureApplicationServices();

            builder.Services.ConfigureMiniProfiler(settings);

            builder.Services.AddControllersWithViews();

            builder.Services.Configure<RouteOptions>(opt =>
            {
                opt.ConstraintMap.Add("enum", typeof(EnumRouteConstraint<Divider>));
            });

            var app = builder.Build();

            var connStrBuilder = new SqlConnectionStringBuilder(settings.ConnectionStrings.EntityDataContext);

            app.Logger.LogInformation("Starting BitPantry.Iota.Web :: {Environment}, {DataSource}", builder.Environment.EnvironmentName, connStrBuilder.DataSource);

            app.UseMiddleware<IotaLogEnricherMiddleware>();

            // Configure the HTTP request pipeline.

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiniProfiler();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .RequireAuthorization();

            app.UseMiddleware<AppStateCookieMiddleware>();

            app.Run();
        }
    }
}


//https://www.youtube.com/watch?v=lxJutCKH1fs
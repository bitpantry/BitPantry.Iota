using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using BitPantry.Iota.Infrastructure.Settings;
using BitPantry.Iota.Web.IoC;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Application.IoC;

namespace BitPantry.Iota.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // configure logging

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // configure services

            var settings = new AppSettings(builder.Configuration);

            builder.Services.ConfigureWebServices(settings);
            builder.Services.ConfigureIdentityServices(settings);
            builder.Services.ConfigureInfrastructureServices(settings, CachingStrategy.InMemory);
            builder.Services.ConfigureApplicationServices();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

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
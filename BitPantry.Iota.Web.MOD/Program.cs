using BitPantry.Iota.Infrastructure.Settings;
using BitPantry.Iota.Infrastructure.IoC;
using BitPantry.Iota.Application.IoC;
using BitPantry.Iota.Web.Identity;
using BitPantry.Iota.Web.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // create builder

            var builder = WebApplication.CreateBuilder(args);

            // configure logging

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // configure services

            var settings = new AppSettings(builder.Configuration);

            builder.Services.ConfigureIdentityServices(settings);
            builder.Services.ConfigureInfrastructureServices(settings, CachingStrategy.InMemory);
            builder.Services.ConfigureApplicationServices();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                // app.UseMigrationsEndPoint();
            }
            else
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
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}

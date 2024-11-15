using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace BitPantry.Iota.Web.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureIdentityServices(this IServiceCollection services, AppSettings settings)
        {
            services.AddAuthentication(o => 
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(builder => {
                builder.LoginPath = "/auth";
            })
            .AddGoogle(GoogleDefaults.AuthenticationScheme, o =>
            {
                o.ClientId = settings.Identity.Google.ClientId;
                o.ClientSecret = settings.Identity.Google.ClientSecret;
            });

            return services;
        }

        public static IServiceCollection ConfigureWebServices(this IServiceCollection services, AppSettings settings)
        {
            // components

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<AppStateCookie>();
            services.AddScoped<UserIdentity>();

            // data protection

            // see https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/using-data-protection?view=aspnetcore-8.0
            // for reference on how data protection is configured with custom key store below
            services.AddDataProtection();
            services.AddOptions<KeyManagementOptions>()
                .Configure<IServiceScopeFactory>((options, factory) => options.XmlRepository = new DatabaseDataProtectionKeyStore(factory));

            // application cookies

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true; // Prevents client-side script access
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensures the cookie is only sent over HTTPS
                options.Cookie.SameSite = SameSiteMode.Strict; // Helps prevent cross-site request forgery
            });

            // services

            services.AddScoped<UserTimeService>();

            return services;
        }

        public static void ConfigureMiniProfiler(this IServiceCollection services, AppSettings settings)
        {
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomRight;
            }).AddEntityFramework();
        }

    }

    
}

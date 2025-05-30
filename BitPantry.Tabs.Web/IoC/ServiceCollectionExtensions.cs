﻿using BitPantry.Tabs.Infrastructure.Settings;
using BitPantry.Tabs.Web.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BitPantry.Tabs.Web.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureWebIdentityServices(this IServiceCollection services, WebAppSettings settings)
        {
            services.AddAuthentication(o =>
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.LoginPath = "/auth";
                })
                .AddGoogle(GoogleDefaults.AuthenticationScheme, opt =>
                {
                    opt.ClientId = settings.Identity.Google.ClientId;
                    opt.ClientSecret = settings.Identity.Google.ClientSecret;
                })
                .AddMicrosoftAccount(opt =>
                {
                    opt.ClientId = settings.Identity.Microsoft.ClientId;
                    opt.ClientSecret = settings.Identity.Microsoft.ClientSecret;
                });

            return services;
        }

        public static IServiceCollection ConfigureWebServices(this IServiceCollection services, WebAppSettings settings)
        {
            // components

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<AppStateCookie>();
            services.AddScoped<UserIdentity>();
            services.AddScoped<CurrentUser>();
            services.AddScoped<TestSettings>();

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

            // middleware

            services.AddScoped<AppStateCookieMiddleware>();
            services.AddScoped<TimeZoneMiddleware>();
            services.AddScoped<TestInfrastructureMiddleware>();

            // services

            services.AddScoped<UserTimeService>();

            return services;
        }

        public static void ConfigureMiniProfiler(this IServiceCollection services, WebAppSettings settings)
        {
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomRight;
            }).AddEntityFramework();
        }

    }

    
}

using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace BitPantry.Iota.Web.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureIdentityServices(this IServiceCollection services, AppSettings settings)
        {
            services.AddAuthentication(o => 
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie()
            .AddGoogle(GoogleDefaults.AuthenticationScheme, o =>
            {
                o.ClientId = "243088652396-lmumnpr93c3enkk0cphcq39epgc1h51g.apps.googleusercontent.com";
                o.ClientSecret = "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";
            });

            return services;
        }
    }
}

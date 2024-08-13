using Microsoft.AspNetCore.Identity;

namespace BitPantry.Iota.Web.Identity
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureIdentityServices(
            this IServiceCollection services)
        {
            services.AddScoped<IUserStore<IotaWebUser>, IotaWebUserStore>();

            services.AddDefaultIdentity<IotaWebUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddUserStore<IotaWebUserStore>();
        }
    }
}

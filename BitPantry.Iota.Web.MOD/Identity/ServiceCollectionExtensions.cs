using BitPantry.Iota.Infrastructure.Settings;
using BitPantry.Iota.Web.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BitPantry.Iota.Web.Identity
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureIdentityServices(
            this IServiceCollection services,
            AppSettings settings)
        {
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(settings.ConnectionStrings.EntityDataContext));

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<IdentityDbContext>();
        }
    }
}

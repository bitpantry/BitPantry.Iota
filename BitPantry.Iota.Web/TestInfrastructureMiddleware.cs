
using BitPantry.Iota.Infrastructure.Settings;
using Microsoft.AspNetCore.Http.Extensions;
using System.CodeDom;

namespace BitPantry.Iota.Web
{
    public class TestInfrastructureMiddleware : IMiddleware
    {
        private readonly AppSettings _appSettings;
        private readonly TestSettings _testSettings;

        public TestInfrastructureMiddleware(AppSettings appSettings, TestSettings testSettings)
        {
            _appSettings = appSettings;
            _testSettings = testSettings;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!_appSettings.EnableTestInfrastructure)
                throw new InvalidOperationException("Test infrastructure is disabled");

            if (context.Request.Cookies.TryGetValue(Constants.USER_CURRENT_TIME_OVERRIDE_UTC_KEY, out var userCurrentTimeTestOverrideUtc))
                _testSettings.UserCurrentTimeUtc = DateTime.Parse(Uri.UnescapeDataString(userCurrentTimeTestOverrideUtc));

            if (context.Request.Cookies.TryGetValue(Constants.TIMEZONE_OVERRIDE_KEY, out var userTimezoneTestOverride))
                _testSettings.UserTimezone = userTimezoneTestOverride;
         
            await next(context);
        }
    }
}

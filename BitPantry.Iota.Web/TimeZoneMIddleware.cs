using BitPantry.Iota.Web.Controllers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Policy;

namespace BitPantry.Iota.Web
{
    public class TimeZoneMiddleware : IMiddleware
    {
        private const string ADD_COOKIE_URL_PATH = "/home/addtimezonecookie";

        private readonly ILogger<TimeZoneMiddleware> _logger;

        public TimeZoneMiddleware(ILogger<TimeZoneMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _logger.LogTrace($"{nameof(TimeZoneMiddleware)}:{nameof(InvokeAsync)}");

            // Try to get the time zone from the cookie
            if (context.Request.Cookies.TryGetValue(Constants.TIMEZONE_KEY_NAME, out var timeZone))
            {
                context.Items[Constants.TIMEZONE_KEY_NAME] = timeZone ?? null;
                await next(context);
            }
            else 
            {
                _logger.LogDebug($"Time zone cookie not found - redirecting to {ADD_COOKIE_URL_PATH} to acquire cookie");

                if (context.Request.Path.Equals(ADD_COOKIE_URL_PATH, StringComparison.InvariantCultureIgnoreCase))
                    await next(context);
                else
                    context.Response.Redirect($"{ADD_COOKIE_URL_PATH}?redirecturl={context.Request.GetEncodedPathAndQuery()}");
                
                return;
            }

        }
    }
}

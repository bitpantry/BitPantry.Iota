﻿using BitPantry.Tabs.Web.Controllers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Policy;

namespace BitPantry.Tabs.Web
{
    public class TimeZoneMiddleware : IMiddleware
    {
        private const string ADD_COOKIE_URL_PATH = "/home/addtimezonecookie";

        private readonly ILogger<TimeZoneMiddleware> _logger;

        public TimeZoneMiddleware(ILogger<TimeZoneMiddleware> logger, CurrentUser currentUser)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.User != null && context.User.Identity.IsAuthenticated)
            {
                // Try to get the time zone from the cookie
                if (context.Request.Cookies.TryGetValue(Constants.TIMEZONE_KEY_NAME, out var timeZone))
                {
                    context.Items[Constants.TIMEZONE_KEY_NAME] = timeZone ?? null;
                    await next(context);
                }
                else
                {
                    if (context.Request.Path.Equals(ADD_COOKIE_URL_PATH, StringComparison.InvariantCultureIgnoreCase))
                        await next(context);
                    else
                        context.Response.Redirect($"{ADD_COOKIE_URL_PATH}?redirecturl={context.Request.GetEncodedPathAndQuery()}");

                    return;
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}

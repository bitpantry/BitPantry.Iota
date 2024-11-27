namespace BitPantry.Iota.Web
{
    public class TimeZoneMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Try to get the time zone from the cookie
            if (context.Request.Cookies.TryGetValue(Constants.TIMEZONE_KEY_NAME, out var timeZone))
                context.Items[Constants.TIMEZONE_KEY_NAME] = timeZone ?? null;

            await next(context);
        }
    }
}

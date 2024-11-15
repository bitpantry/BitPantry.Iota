namespace BitPantry.Iota.Web
{
    public class TimeZoneMiddleware
    {
        private readonly RequestDelegate _next;

        public TimeZoneMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Try to get the time zone from the cookie
            if (context.Request.Cookies.TryGetValue(Constants.TIMEZONE_KEY_NAME, out var timeZone))
                context.Items[Constants.TIMEZONE_KEY_NAME] = timeZone ?? null;

            await _next(context);
        }
    }
}

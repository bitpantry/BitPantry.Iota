namespace BitPantry.Iota.Web
{
    public class AppStateCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public AppStateCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppStateCookie appStateCookie)
        {
            await _next(context);
            appStateCookie.UpdateCookie();
        }
    }
}

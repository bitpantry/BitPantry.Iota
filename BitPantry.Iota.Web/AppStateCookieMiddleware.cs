namespace BitPantry.Iota.Web
{
    public class AppStateCookieMiddleware : IMiddleware
    {
        private AppStateCookie _appStateCookie;

        public AppStateCookieMiddleware(AppStateCookie appStateCookie)
        {
            _appStateCookie = appStateCookie;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);
            _appStateCookie.PersistCookie();
        }
    }
}

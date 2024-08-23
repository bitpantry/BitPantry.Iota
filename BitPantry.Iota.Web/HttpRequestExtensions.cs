﻿namespace BitPantry.Iota.Web
{
    public static class HttpRequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
            => request.Headers.XRequestedWith == "XMLHttpRequest";
    }
}

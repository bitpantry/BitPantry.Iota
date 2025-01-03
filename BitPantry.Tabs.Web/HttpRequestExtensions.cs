using Azure.Core;
using System.Text.RegularExpressions;

namespace BitPantry.Tabs.Web
{
    public enum CardMaintenanceReferrer : int
    {
        Other = 0,
        CardMaintenance = 1,
        CollectionTab = 2
    }

    public static class HttpRequestExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
            => request.Headers.XRequestedWith == "XMLHttpRequest";

        public static CardMaintenanceReferrer GetCardMaintenanceReferrer(this HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Referrer"))
            {
                var referrer = request.Headers["Referer"].ToString();

                if (new Regex("\\/card\\/\\d{1,19}$").IsMatch(referrer)) return CardMaintenanceReferrer.CardMaintenance;
                if (new Regex("\\/collection\\/[a-zA-Z0-9_-]+$").IsMatch(referrer)) return CardMaintenanceReferrer.CollectionTab;
            }

            return CardMaintenanceReferrer.Other;
        }
    }
}

using BitPantry.Tabs.Infrastructure.Settings;
using BitPantry.Tabs.Web.Settings;

namespace BitPantry.Tabs.Web
{
    public class UserTimeService
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly WebAppSettings _appSettings;
        private readonly TestSettings _testSettings;

        public UserTimeService(IHttpContextAccessor httpContextAccessor, WebAppSettings appSettings, TestSettings testSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _appSettings = appSettings;
            _testSettings = testSettings;
        }

        public string UserTimezone
            => _appSettings.EnableTestInfrastructure
                ? _testSettings.UserTimezone ?? _httpContextAccessor.HttpContext.Items[Constants.TIMEZONE_KEY_NAME] as string
                : _httpContextAccessor.HttpContext.Items[Constants.TIMEZONE_KEY_NAME] as string; 

        public DateTime ConvertUtcToUserLocalTime(DateTime utcDateTime)
        {
            // Ensure the DateTime is in UTC
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            // Find the user's time zone
            TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(UserTimezone ?? "utc");

            // Convert the UTC DateTime to the user's local time zone
            DateTime userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, userTimeZone);

            return userLocalTime;
        }

        // Overload to get the current UTC DateTime in the user's local time zone
        public DateTime GetCurrentUserLocalTime()
        {
            DateTime utcNow = _appSettings.EnableTestInfrastructure ? _testSettings.UserCurrentTimeUtc : DateTime.UtcNow;
            return ConvertUtcToUserLocalTime(utcNow);
        }

    }
}

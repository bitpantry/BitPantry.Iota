namespace BitPantry.Iota.Web
{
    public class UserTimeService
    {
        private IHttpContextAccessor _httpContextAccessor;

        public UserTimeService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserTimezone
            => _httpContextAccessor.HttpContext.Items[Constants.TIMEZONE_KEY_NAME] as string; // ?? "American/Chicago";

        public DateTime ConvertUtcToUserLocalTime(DateTime utcDateTime)
        {
            // Ensure the DateTime is in UTC
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            // Find the user's time zone
            TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(UserTimezone);

            // Convert the UTC DateTime to the user's local time zone
            DateTime userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, userTimeZone);

            return userLocalTime;
        }

        // Overload to get the current UTC DateTime in the user's local time zone
        public DateTime GetCurrentUserLocalTime()
        {
            DateTime utcNow = DateTime.UtcNow;
            return ConvertUtcToUserLocalTime(utcNow);
        }

    }
}

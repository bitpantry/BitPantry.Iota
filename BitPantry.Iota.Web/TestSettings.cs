using BitPantry.Iota.Infrastructure.Settings;

namespace BitPantry.Iota.Web
{
    public class TestSettings
    {
        private readonly AppSettings _appSettings;

        private DateTime? _userCurrentTimeUtcOverride = null;
        public DateTime UserCurrentTimeUtc
        {
            get
            {
                EnsureTestInfrastructureIsEnabled();
                return _userCurrentTimeUtcOverride ?? DateTime.UtcNow;
            }
            set
            {
                EnsureTestInfrastructureIsEnabled();
                _userCurrentTimeUtcOverride = value;
            }
        }

        private string _userTimezoneOverride; // "America/Chicago";
        public string UserTimezone
        {
            get
            {
                EnsureTestInfrastructureIsEnabled();
                return _userTimezoneOverride;
            }
            set
            {
                EnsureTestInfrastructureIsEnabled();
                _userTimezoneOverride = value;
            }
        }

        public TestSettings(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        private void EnsureTestInfrastructureIsEnabled()
        {
            if (!_appSettings.EnableTestInfrastructure)
                throw new InvalidOperationException("Test infrastructure is disabled.");
        }
    }
}

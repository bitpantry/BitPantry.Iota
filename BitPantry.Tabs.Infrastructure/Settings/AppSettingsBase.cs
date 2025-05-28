using BitPantry.Parsing.Strings;
using Microsoft.Extensions.Configuration;

namespace BitPantry.Tabs.Infrastructure.Settings
{
    public abstract class AppSettingsBase
    {
        protected IConfiguration Config { get; private set; }

        private readonly string _keyPrefix;

        protected AppSettingsBase(IConfiguration config) : this(config, null) { }

        protected AppSettingsBase(
            IConfiguration config,
            string keyPrefix)
        {
            Config = config;
            _keyPrefix = keyPrefix;

        }

        protected string GetValue(string key)
        {
            try
            {
                return GetValue<string>(key);
            }
            catch (Exception ex)
            {
                throw CreateAppConfigurationException(key, ex);
            }
        }

        protected TType GetValue<TType>(string key)
        {
            try
            {
                return StringParsing.Parse<TType>(Config[FormatKey(key)]);
            }
            catch (Exception ex)
            {
                throw CreateAppConfigurationException(key, ex);
            }
        }

        protected TType GetValue<TType>(string key, TType defaultValue)
        {
            try
            {
                return StringParsing.SafeParse(Config[FormatKey(key)], defaultValue);
            }
            catch (Exception ex)
            {
                throw CreateAppConfigurationException(key, ex);
            }
        }

        private string FormatKey(string key) { return string.IsNullOrEmpty(_keyPrefix) ? key : $"{_keyPrefix}:{key}"; }

        private AppSettingsException CreateAppConfigurationException(string key, Exception innerException)
        {
            return new AppSettingsException(
                $"An application setting error occurred while getting the value for key \"{FormatKey(key)}\"",
                innerException);
        }
    }
}

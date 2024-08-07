using System;

namespace BitPantry.Iota.Infrastructure.Settings
{
    public class AppSettingsException : Exception
    {
        public AppSettingsException(string message, Exception innerException) : base(message, innerException) { }
    }
}

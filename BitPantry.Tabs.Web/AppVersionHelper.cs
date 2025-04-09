using System.Reflection;

namespace BitPantry.Tabs.Web
{
    public static class AppVersionHelper
    {
        public static string GetInformationalVersion() =>
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "unknown";
    }
}

using System.Reflection;

namespace Automind.Treinamentos.Models;

public static class AppVersionInfo
{
    public static string Value { get; } = Resolve();
    public static string Display => $"v{Value}";

    private static string Resolve()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
            return informational.Split('+')[0];

        var version = assembly.GetName().Version;
        return version is null ? "0.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}

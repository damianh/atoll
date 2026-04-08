using System.Reflection;

namespace Atoll.Cli;

/// <summary>
/// Provides CLI metadata such as the product version derived from the assembly's
/// <see cref="AssemblyInformationalVersionAttribute"/> (set by MinVer at build time).
/// </summary>
internal static class CliInfo
{
    /// <summary>
    /// The SemVer version string (e.g. <c>0.6.3</c>) without the git hash suffix.
    /// Falls back to <c>0.0.0</c> when the attribute is missing (e.g. during tests).
    /// </summary>
    internal static string Version { get; } = ResolveVersion();

    private static string ResolveVersion()
    {
        var informational = typeof(CliInfo).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (informational is null)
        {
            return "0.0.0";
        }

        // MinVer produces "x.y.z+commitsha" — strip the metadata suffix.
        var plusIndex = informational.IndexOf('+');
        return plusIndex >= 0 ? informational[..plusIndex] : informational;
    }
}

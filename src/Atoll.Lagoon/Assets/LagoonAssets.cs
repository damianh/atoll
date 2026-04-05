using System.Reflection;

namespace Atoll.Lagoon.Assets;

/// <summary>
/// Provides access to the embedded static assets shipped with <c>Atoll.Lagoon</c>,
/// such as the default Atoll icon SVG used as favicon and header logo.
/// </summary>
public static class LagoonAssets
{
    private static readonly Assembly ResourceAssembly = typeof(LagoonAssets).Assembly;

    /// <summary>The logical resource name for the Atoll icon SVG.</summary>
    public const string IconSvgResourceName = "Atoll.Lagoon.Assets.atoll-icon.svg";

    /// <summary>The URL path used to serve the default favicon in dev mode and SSG output.</summary>
    public const string DefaultFaviconPath = "/_atoll/favicon.svg";

    private static string? _iconSvg;

    /// <summary>
    /// Gets the content of the embedded Atoll icon SVG.
    /// </summary>
    /// <returns>The SVG markup.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the embedded resource cannot be found.
    /// </exception>
    public static string GetIconSvg()
    {
        return _iconSvg ??= ReadResource(IconSvgResourceName);
    }

    private static string ReadResource(string resourceName)
    {
        using var stream = ResourceAssembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            var available = ResourceAssembly.GetManifestResourceNames();
            throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found in assembly " +
                $"'{ResourceAssembly.GetName().Name}'. " +
                $"Available resources: [{string.Join(", ", available)}].");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

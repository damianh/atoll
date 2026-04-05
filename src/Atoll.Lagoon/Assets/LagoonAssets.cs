using System.Reflection;

namespace Atoll.Lagoon.Assets;

/// <summary>
/// Provides access to the embedded static assets shipped with <c>Atoll.Lagoon</c>,
/// such as the default Atoll logo PNG used as favicon and header logo.
/// </summary>
public static class LagoonAssets
{
    private static readonly Assembly ResourceAssembly = typeof(LagoonAssets).Assembly;

    /// <summary>The logical resource name for the Atoll logo PNG.</summary>
    public const string LogoPngResourceName = "Atoll.Lagoon.Assets.logo.png";

    /// <summary>The URL path used to serve the default favicon in dev mode and SSG output.</summary>
    public const string DefaultFaviconPath = "/_atoll/logo.png";

    private static byte[]? _logoPng;

    /// <summary>
    /// Gets the content of the embedded Atoll logo PNG as a byte array.
    /// </summary>
    /// <returns>The PNG image bytes.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the embedded resource cannot be found.
    /// </exception>
    public static byte[] GetLogoPng()
    {
        return _logoPng ??= ReadBinaryResource(LogoPngResourceName);
    }

    private static byte[] ReadBinaryResource(string resourceName)
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

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}

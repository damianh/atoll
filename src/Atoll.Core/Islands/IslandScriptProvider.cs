using System.Reflection;

namespace Atoll.Core.Islands;

/// <summary>
/// Provides access to the embedded island JavaScript assets
/// (<c>atoll-island.js</c> and <c>atoll-directives.js</c>).
/// </summary>
/// <remarks>
/// <para>
/// The JavaScript files are embedded resources in the Atoll.Core assembly.
/// This provider reads them on first access and caches the content for
/// subsequent requests. Hosting middleware uses this to serve the scripts
/// to clients.
/// </para>
/// <para>
/// This is the Atoll equivalent of Astro's runtime script serving in the
/// dev server and build pipeline.
/// </para>
/// </remarks>
public static class IslandScriptProvider
{
    private static readonly Assembly ResourceAssembly = typeof(IslandScriptProvider).Assembly;

    private static string? _islandScript;
    private static string? _directivesScript;

    /// <summary>
    /// The logical resource name for the <c>atoll-island.js</c> script.
    /// </summary>
    public const string IslandScriptResourceName = "Atoll.Core.Islands.Assets.atoll-island.js";

    /// <summary>
    /// The logical resource name for the <c>atoll-directives.js</c> script.
    /// </summary>
    public const string DirectivesScriptResourceName = "Atoll.Core.Islands.Assets.atoll-directives.js";

    /// <summary>
    /// Gets the content of the <c>atoll-island.js</c> script that defines the
    /// <c>&lt;atoll-island&gt;</c> custom element.
    /// </summary>
    /// <returns>The JavaScript source code.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the embedded resource cannot be found.
    /// </exception>
    public static string GetIslandScript()
    {
        return _islandScript ??= ReadResource(IslandScriptResourceName);
    }

    /// <summary>
    /// Gets the content of the <c>atoll-directives.js</c> script that registers
    /// the hydration directive handlers (load, idle, visible, media).
    /// </summary>
    /// <returns>The JavaScript source code.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the embedded resource cannot be found.
    /// </exception>
    public static string GetDirectivesScript()
    {
        return _directivesScript ??= ReadResource(DirectivesScriptResourceName);
    }

    /// <summary>
    /// Gets the content of an embedded resource by its logical name.
    /// </summary>
    /// <param name="resourceName">The logical resource name.</param>
    /// <returns>The resource content as a string.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="resourceName"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the specified resource is not found in the assembly.
    /// </exception>
    public static string GetScript(string resourceName)
    {
        ArgumentNullException.ThrowIfNull(resourceName);

        return ReadResource(resourceName);
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

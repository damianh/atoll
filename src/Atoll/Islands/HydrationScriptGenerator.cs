namespace Atoll.Islands;

/// <summary>
/// Generates the one-time hydration bootstrap script that defines the
/// <c>atoll-island</c> custom element and directive handlers.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's hydration script injection in
/// <c>runtime/server/hydration.ts</c>. The bootstrap script only needs to be
/// included once per page, regardless of how many islands are present.
/// </para>
/// <para>
/// The generated script:
/// </para>
/// <list type="bullet">
/// <item>Defines the <c>atoll-island</c> custom element</item>
/// <item>Includes directive handlers for the specified directive types (load, idle, visible, media)</item>
/// <item>Sets up the prop deserialization and hydration lifecycle</item>
/// </list>
/// </remarks>
public static class HydrationScriptGenerator
{
    /// <summary>
    /// The deduplication key for the island bootstrap script.
    /// Used by the hydration tracker to ensure the script
    /// is only emitted once per page.
    /// </summary>
    public const string BootstrapScriptKey = "atoll:island:bootstrap";

    /// <summary>
    /// Generates an external <c>&lt;script type="module" src="..."&gt;</c> tag that loads the island bootstrap code.
    /// </summary>
    /// <remarks>
    /// Atoll never emits the bootstrap inline so that pages work under a strict
    /// Content-Security-Policy (<c>script-src 'self'</c>).
    /// </remarks>
    /// <param name="islandScriptUrl">The URL of the <c>atoll-island.js</c> script (for example <c>/_atoll/island.js</c>).</param>
    /// <returns>The HTML script tag string.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="islandScriptUrl"/> is <c>null</c>.
    /// </exception>
    public static string GenerateBootstrapScript(string islandScriptUrl)
    {
        ArgumentNullException.ThrowIfNull(islandScriptUrl);

        return $"<script type=\"module\" src=\"{EscapeAttribute(islandScriptUrl)}\"></script>";
    }

    /// <summary>
    /// Generates a <c>&lt;script&gt;</c> tag that imports the directive handler
    /// for the specified directive type.
    /// </summary>
    /// <param name="directiveScriptUrl">The URL of the directive handler script.</param>
    /// <returns>The HTML script tag string.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="directiveScriptUrl"/> is <c>null</c>.
    /// </exception>
    public static string GenerateDirectiveScript(string directiveScriptUrl)
    {
        ArgumentNullException.ThrowIfNull(directiveScriptUrl);

        return $"<script type=\"module\" src=\"{EscapeAttribute(directiveScriptUrl)}\"></script>";
    }

    private static string EscapeAttribute(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}

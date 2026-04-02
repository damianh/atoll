namespace Atoll.Core.Islands;

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
    /// Generates a <c>&lt;script&gt;</c> tag containing the island bootstrap code.
    /// </summary>
    /// <param name="islandScriptUrl">
    /// The URL of the <c>atoll-island.js</c> script. If <c>null</c>, an inline
    /// script with a minimal island definition is emitted.
    /// </param>
    /// <returns>The HTML script tag string.</returns>
    public static string GenerateBootstrapScript(string? islandScriptUrl)
    {
        if (islandScriptUrl is not null)
        {
            return $"<script type=\"module\" src=\"{EscapeAttribute(islandScriptUrl)}\"></script>";
        }

        // Inline minimal island definition when no external script URL is provided
        return "<script type=\"module\">" + InlineBootstrapScript + "</script>";
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

    // Minimal inline bootstrap that defines the atoll-island custom element.
    // In production, this would be the full atoll-island.js content.
    // For now, this is a placeholder that registers the custom element
    // and handles basic hydration coordination.
    private const string InlineBootstrapScript = """
(()=>{if(customElements.get('atoll-island'))return;class A extends HTMLElement{connectedCallback(){this.hasAttribute('ssr')&&this.dispatchEvent(new CustomEvent('atoll:hydrate',{bubbles:!0}))}}customElements.define('atoll-island',A)})()
""";
}

using System.Text.RegularExpressions;

namespace Atoll.Islands;

/// <summary>
/// Scans rendered HTML for <c>&lt;atoll-island&gt;</c> elements and extracts
/// the distinct set of client directive types in use. Used by the page renderer
/// to determine which hydration scripts need to be injected.
/// </summary>
public static partial class IslandDirectiveScanner
{
    /// <summary>
    /// The URL for the island bootstrap script (custom element definition).
    /// </summary>
    internal const string IslandScriptUrl = "/_atoll/island.js";

    /// <summary>
    /// The URL for the combined directives script (load, idle, visible, media handlers).
    /// </summary>
    internal const string DirectivesScriptUrl = "/_atoll/directives.js";

    /// <summary>
    /// Scans the HTML for any <c>&lt;atoll-island client="..."&gt;</c> elements.
    /// </summary>
    /// <param name="html">The rendered HTML to scan.</param>
    /// <returns><c>true</c> if at least one island element was found; otherwise, <c>false</c>.</returns>
    public static bool ContainsIslands(string html)
    {
        ArgumentNullException.ThrowIfNull(html);

        return IslandClientAttributeRegex().IsMatch(html);
    }

    /// <summary>
    /// Matches <c>client="..."</c> attribute values on <c>&lt;atoll-island&gt;</c> elements.
    /// </summary>
    [GeneratedRegex("""<atoll-island\b[^>]*?\bclient="([^"]+)"[^>]*>""", RegexOptions.IgnoreCase)]
    private static partial Regex IslandClientAttributeRegex();
}

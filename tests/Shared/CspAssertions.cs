using System.Text.RegularExpressions;

namespace Atoll.Tests.Shared;

/// <summary>
/// Assertions that rendered HTML is compatible with a strict Content-Security-Policy
/// (<c>script-src 'self'</c>): no executable inline scripts and no inline event handlers.
/// </summary>
internal static partial class CspAssertions
{
    private static readonly HashSet<string> DataBlockTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/json",
        "application/ld+json",
    };

    public static void ShouldBeStrictCspCompatible(this string html)
    {
        ArgumentNullException.ThrowIfNull(html);

        var violations = new List<string>();

        foreach (Match match in ScriptRegex().Matches(html))
        {
            if (string.IsNullOrWhiteSpace(match.Groups["body"].Value))
            {
                continue;
            }

            var type = TypeAttributeRegex().Match(match.Groups["attrs"].Value);
            if (type.Success && DataBlockTypes.Contains(type.Groups["type"].Value))
            {
                continue;
            }

            violations.Add("inline <script>: " + Truncate(match.Value));
        }

        var withoutScriptBodies = ScriptRegex().Replace(html, m => "<script" + m.Groups["attrs"].Value + "></script>");
        foreach (Match match in EventHandlerAttributeRegex().Matches(withoutScriptBodies))
        {
            violations.Add("inline event handler: " + Truncate(match.Value));
        }

        violations.ShouldBeEmpty(
            "HTML must not contain inline scripts or on* handlers (strict CSP):\n" + string.Join("\n", violations));
    }

    private static string Truncate(string value) =>
        value.Length <= 200 ? value : value[..200] + "…";

    [GeneratedRegex(@"<script\b(?<attrs>[^>]*)>(?<body>.*?)</script\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptRegex();

    [GeneratedRegex("""\btype\s*=\s*["']?(?<type>[^"'\s>]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex TypeAttributeRegex();

    // An on* attribute inside a tag, e.g. <button onclick="...">.
    [GeneratedRegex("""<[a-zA-Z][^<>]*?\son[a-z]+\s*=[^<>]*>""", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerAttributeRegex();
}

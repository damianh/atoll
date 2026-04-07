using System.Globalization;
using System.Text.RegularExpressions;

namespace Atoll.Lagoon.Navigation;

/// <summary>
/// Utility methods for converting path segment strings into human-readable sidebar labels.
/// </summary>
internal static partial class SlugLabelHelper
{
    // Matches a leading sequence of digits followed by a hyphen: e.g., "01-", "123-"
    [GeneratedRegex(@"^\d+-", RegexOptions.CultureInvariant)]
    private static partial Regex NumericPrefixPattern();

    /// <summary>
    /// Strips a leading numeric prefix (e.g., <c>"01-"</c>, <c>"02-"</c>) from a path segment.
    /// </summary>
    /// <param name="segment">The path segment to process (e.g., <c>"01-intro"</c>).</param>
    /// <returns>
    /// The segment with the numeric prefix removed (e.g., <c>"intro"</c>),
    /// or the original string if no numeric prefix is present.
    /// </returns>
    /// <example>
    /// <code>
    /// SlugLabelHelper.StripNumericPrefix("01-intro")     // "intro"
    /// SlugLabelHelper.StripNumericPrefix("no-prefix")    // "no-prefix"
    /// SlugLabelHelper.StripNumericPrefix("123-")         // ""
    /// </code>
    /// </example>
    internal static string StripNumericPrefix(string segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        return NumericPrefixPattern().Replace(segment, "");
    }

    /// <summary>
    /// Converts a path segment into a human-readable label by stripping any numeric prefix,
    /// replacing hyphens and underscores with spaces, and applying title-case.
    /// </summary>
    /// <param name="segment">The path segment to humanise (e.g., <c>"02-getting-started"</c>).</param>
    /// <returns>
    /// A human-readable label (e.g., <c>"Getting Started"</c>).
    /// Returns an empty string when the input is empty or results in an empty string after processing.
    /// </returns>
    /// <example>
    /// <code>
    /// SlugLabelHelper.Humanize("02-getting-started") // "Getting Started"
    /// SlugLabelHelper.Humanize("api_reference")      // "Api Reference"
    /// SlugLabelHelper.Humanize("intro")              // "Intro"
    /// </code>
    /// </example>
    internal static string Humanize(string segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        if (string.IsNullOrEmpty(segment))
        {
            return segment;
        }

        var stripped = StripNumericPrefix(segment);
        if (string.IsNullOrEmpty(stripped))
        {
            return stripped;
        }

        // Replace hyphens and underscores with spaces, then title-case each word.
        var words = stripped.Replace('-', ' ').Replace('_', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var textInfo = CultureInfo.InvariantCulture.TextInfo;
        return string.Join(" ", words.Select(w => textInfo.ToTitleCase(w.ToLowerInvariant())));
    }

    /// <summary>
    /// Attempts to parse a numeric prefix from a path segment (e.g., <c>"01-basics"</c> → <c>1</c>).
    /// </summary>
    /// <param name="segment">The path segment to inspect.</param>
    /// <param name="prefix">
    /// When the method returns <c>true</c>, contains the parsed numeric value of the prefix.
    /// </param>
    /// <returns>
    /// <c>true</c> if the segment starts with a numeric prefix (e.g., <c>"01-"</c>); otherwise <c>false</c>.
    /// </returns>
    internal static bool TryParseNumericPrefix(string segment, out int prefix)
    {
        ArgumentNullException.ThrowIfNull(segment);
        var match = NumericPrefixPattern().Match(segment);
        if (!match.Success)
        {
            prefix = 0;
            return false;
        }

        // The match includes the trailing hyphen; strip it before parsing.
        var numberPart = match.Value.TrimEnd('-');
        return int.TryParse(numberPart, NumberStyles.None, CultureInfo.InvariantCulture, out prefix);
    }
}

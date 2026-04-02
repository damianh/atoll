namespace Atoll.Routing.FileSystem;

/// <summary>
/// Defines the type of a route segment in a file-based routing pattern.
/// </summary>
public enum RouteSegmentType
{
    /// <summary>
    /// A static segment that matches exactly (e.g., <c>blog</c> in <c>/blog/hello</c>).
    /// </summary>
    Static,

    /// <summary>
    /// A dynamic segment that captures a single path segment (e.g., <c>[slug]</c> in <c>/blog/[slug]</c>).
    /// </summary>
    Dynamic,

    /// <summary>
    /// A catch-all segment that captures the remaining path (e.g., <c>[...rest]</c> in <c>/docs/[...rest]</c>).
    /// </summary>
    CatchAll
}

/// <summary>
/// Represents a single segment within a route pattern, with its type and parameter name.
/// </summary>
public sealed class RouteSegment
{
    /// <summary>
    /// Initializes a new <see cref="RouteSegment"/> with the specified type and value.
    /// </summary>
    /// <param name="segmentType">The type of this segment.</param>
    /// <param name="value">
    /// For <see cref="RouteSegmentType.Static"/> segments, the literal path text.
    /// For <see cref="RouteSegmentType.Dynamic"/> and <see cref="RouteSegmentType.CatchAll"/> segments,
    /// the parameter name (e.g., <c>slug</c> from <c>[slug]</c>).
    /// </param>
    public RouteSegment(RouteSegmentType segmentType, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        SegmentType = segmentType;
        Value = value;
    }

    /// <summary>
    /// Gets the type of this segment.
    /// </summary>
    public RouteSegmentType SegmentType { get; }

    /// <summary>
    /// Gets the value of this segment.
    /// For static segments, this is the literal text. For dynamic/catch-all segments, this is the parameter name.
    /// </summary>
    public string Value { get; }
}

/// <summary>
/// Provides file-based routing conventions that map file paths to URL route patterns.
/// </summary>
/// <remarks>
/// <para>
/// This implements Astro-style file-based routing conventions:
/// </para>
/// <list type="bullet">
/// <item><description><c>index.cs</c> → <c>/</c></description></item>
/// <item><description><c>about.cs</c> → <c>/about</c></description></item>
/// <item><description><c>blog/index.cs</c> → <c>/blog</c></description></item>
/// <item><description><c>blog/[slug].cs</c> → <c>/blog/[slug]</c></description></item>
/// <item><description><c>[...rest].cs</c> → <c>/[...rest]</c></description></item>
/// </list>
/// </remarks>
public static class RouteConventions
{
    /// <summary>
    /// The file extension that identifies page/endpoint source files.
    /// </summary>
    public const string PageFileExtension = ".cs";

    /// <summary>
    /// The index file name (without extension) that maps to the directory root.
    /// </summary>
    public const string IndexFileName = "index";

    /// <summary>
    /// Converts a relative file path to a URL route pattern.
    /// </summary>
    /// <param name="relativeFilePath">
    /// The file path relative to the pages directory, using forward slashes
    /// (e.g., <c>blog/[slug].cs</c>).
    /// </param>
    /// <returns>
    /// The URL route pattern (e.g., <c>/blog/[slug]</c>).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the file path does not end with <see cref="PageFileExtension"/>.
    /// </exception>
    public static string FilePathToPattern(string relativeFilePath)
    {
        ArgumentNullException.ThrowIfNull(relativeFilePath);

        if (!relativeFilePath.EndsWith(PageFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"File path must end with '{PageFileExtension}'.",
                nameof(relativeFilePath));
        }

        // Normalize separators to forward slashes
        var normalized = relativeFilePath.Replace('\\', '/');

        // Remove the file extension
        var withoutExtension = normalized[..^PageFileExtension.Length];

        // Remove trailing "index" — index files map to the directory root
        if (withoutExtension.Equals(IndexFileName, StringComparison.OrdinalIgnoreCase))
        {
            return "/";
        }

        if (withoutExtension.EndsWith("/" + IndexFileName, StringComparison.OrdinalIgnoreCase))
        {
            withoutExtension = withoutExtension[..^(IndexFileName.Length + 1)];
        }

        // Ensure leading slash
        return "/" + withoutExtension;
    }

    /// <summary>
    /// Parses a URL route pattern into its constituent segments.
    /// </summary>
    /// <param name="pattern">The URL route pattern (e.g., <c>/blog/[slug]</c>).</param>
    /// <returns>An array of <see cref="RouteSegment"/> values describing the pattern.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when a segment has invalid bracket syntax.
    /// </exception>
    public static RouteSegment[] ParseSegments(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        // Root pattern has no segments
        if (pattern == "/")
        {
            return [];
        }

        // Remove leading slash
        var trimmed = pattern.TrimStart('/');
        var parts = trimmed.Split('/');
        var segments = new RouteSegment[parts.Length];

        for (var i = 0; i < parts.Length; i++)
        {
            segments[i] = ParseSingleSegment(parts[i], i, parts.Length);
        }

        return segments;
    }

    /// <summary>
    /// Determines whether the specified file name (without extension) represents
    /// a dynamic route segment.
    /// </summary>
    /// <param name="fileNameWithoutExtension">The file/directory name to check.</param>
    /// <returns><c>true</c> if the name represents a dynamic segment; otherwise, <c>false</c>.</returns>
    public static bool IsDynamicSegment(string fileNameWithoutExtension)
    {
        ArgumentNullException.ThrowIfNull(fileNameWithoutExtension);
        return fileNameWithoutExtension.StartsWith('[') && fileNameWithoutExtension.EndsWith(']');
    }

    /// <summary>
    /// Determines whether the specified file name (without extension) represents
    /// a catch-all route segment.
    /// </summary>
    /// <param name="fileNameWithoutExtension">The file/directory name to check.</param>
    /// <returns><c>true</c> if the name represents a catch-all segment; otherwise, <c>false</c>.</returns>
    public static bool IsCatchAllSegment(string fileNameWithoutExtension)
    {
        ArgumentNullException.ThrowIfNull(fileNameWithoutExtension);
        return fileNameWithoutExtension.StartsWith("[...", StringComparison.Ordinal)
               && fileNameWithoutExtension.EndsWith(']');
    }

    /// <summary>
    /// Extracts the parameter name from a dynamic or catch-all segment.
    /// </summary>
    /// <param name="segment">The segment text (e.g., <c>[slug]</c> or <c>[...rest]</c>).</param>
    /// <returns>The parameter name (e.g., <c>slug</c> or <c>rest</c>).</returns>
    /// <exception cref="ArgumentException">Thrown when the segment is not a valid dynamic or catch-all segment.</exception>
    public static string ExtractParameterName(string segment)
    {
        ArgumentNullException.ThrowIfNull(segment);

        if (IsCatchAllSegment(segment))
        {
            // [...rest] → rest
            return segment[4..^1];
        }

        if (IsDynamicSegment(segment))
        {
            // [slug] → slug
            return segment[1..^1];
        }

        throw new ArgumentException(
            $"Segment '{segment}' is not a dynamic or catch-all segment.",
            nameof(segment));
    }

    private static RouteSegment ParseSingleSegment(string segment, int index, int totalCount)
    {
        if (IsCatchAllSegment(segment))
        {
            if (index != totalCount - 1)
            {
                throw new ArgumentException(
                    $"Catch-all segment '[...{ExtractParameterName(segment)}]' must be the last segment in the pattern.");
            }

            var paramName = ExtractParameterName(segment);
            if (string.IsNullOrEmpty(paramName))
            {
                throw new ArgumentException("Catch-all segment must have a parameter name (e.g., '[...rest]').");
            }

            return new RouteSegment(RouteSegmentType.CatchAll, paramName);
        }

        if (IsDynamicSegment(segment))
        {
            var paramName = ExtractParameterName(segment);
            if (string.IsNullOrEmpty(paramName))
            {
                throw new ArgumentException("Dynamic segment must have a parameter name (e.g., '[slug]').");
            }

            return new RouteSegment(RouteSegmentType.Dynamic, paramName);
        }

        return new RouteSegment(RouteSegmentType.Static, segment);
    }
}

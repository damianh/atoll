using Atoll.Routing.FileSystem;

namespace Atoll.Routing.Matching;

/// <summary>
/// Represents a parsed route pattern ready for URL matching.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="RoutePattern"/> is the compiled form of a route pattern string
/// (e.g., <c>/blog/[slug]</c>). It contains the pre-parsed segments for efficient
/// matching against incoming URL paths.
/// </para>
/// </remarks>
public sealed class RoutePattern
{
    /// <summary>
    /// Initializes a new <see cref="RoutePattern"/> from the specified pattern string.
    /// </summary>
    /// <param name="pattern">The URL pattern string (e.g., <c>/blog/[slug]</c>).</param>
    public RoutePattern(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        RawPattern = pattern;
        Segments = RouteConventions.ParseSegments(pattern);
    }

    /// <summary>
    /// Gets the raw pattern string.
    /// </summary>
    public string RawPattern { get; }

    /// <summary>
    /// Gets the parsed segments of this pattern.
    /// </summary>
    public RouteSegment[] Segments { get; }

    /// <summary>
    /// Gets a value indicating whether this pattern has any dynamic (non-static) segments.
    /// </summary>
    public bool IsDynamic => Segments.Any(s => s.SegmentType != RouteSegmentType.Static);

    /// <summary>
    /// Gets a value indicating whether this pattern ends with a catch-all segment.
    /// </summary>
    public bool HasCatchAll =>
        Segments.Length > 0 && Segments[^1].SegmentType == RouteSegmentType.CatchAll;

    /// <summary>
    /// Gets the number of static segments in this pattern.
    /// </summary>
    public int StaticSegmentCount => Segments.Count(s => s.SegmentType == RouteSegmentType.Static);

    /// <summary>
    /// Gets the number of dynamic (single-value) segments in this pattern.
    /// </summary>
    public int DynamicSegmentCount => Segments.Count(s => s.SegmentType == RouteSegmentType.Dynamic);
}

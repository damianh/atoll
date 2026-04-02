using Atoll.Routing.FileSystem;

namespace Atoll.Routing.Matching;

/// <summary>
/// Compares route patterns for priority ordering.
/// </summary>
/// <remarks>
/// <para>
/// Route priority follows Astro conventions (most specific first):
/// </para>
/// <list type="number">
/// <item><description>Fully static routes (no dynamic segments) come first.</description></item>
/// <item><description>Routes with more static segments come before routes with fewer.</description></item>
/// <item><description>Routes with dynamic segments come before routes with catch-all segments.</description></item>
/// <item><description>Among routes with equal specificity, shorter patterns come first.</description></item>
/// <item><description>Catch-all routes come last.</description></item>
/// </list>
/// </remarks>
public static class RouteComparer
{
    /// <summary>
    /// Compares two route patterns for priority ordering.
    /// Returns a negative value if <paramref name="x"/> has higher priority (should come first),
    /// a positive value if <paramref name="y"/> has higher priority, or zero if equal.
    /// </summary>
    /// <param name="x">The first route pattern.</param>
    /// <param name="y">The second route pattern.</param>
    /// <returns>
    /// A value less than zero if <paramref name="x"/> should come before <paramref name="y"/>,
    /// greater than zero if <paramref name="y"/> should come before <paramref name="x"/>,
    /// or zero if they have equal priority.
    /// </returns>
    public static int Compare(RoutePattern x, RoutePattern y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        // 1. Routes without catch-all come before routes with catch-all
        var xCatchAll = x.HasCatchAll;
        var yCatchAll = y.HasCatchAll;
        if (xCatchAll != yCatchAll)
        {
            return xCatchAll ? 1 : -1;
        }

        // 2. More total segments (excluding catch-all) = more specific = higher priority
        var xNonCatchAllCount = GetNonCatchAllSegmentCount(x);
        var yNonCatchAllCount = GetNonCatchAllSegmentCount(y);
        if (xNonCatchAllCount != yNonCatchAllCount)
        {
            return yNonCatchAllCount.CompareTo(xNonCatchAllCount); // More segments = higher priority
        }

        // 3. More static segments = more specific = higher priority
        var staticComparison = y.StaticSegmentCount.CompareTo(x.StaticSegmentCount);
        if (staticComparison != 0)
        {
            return staticComparison;
        }

        // 4. Fewer dynamic segments = more specific = higher priority
        var dynamicComparison = x.DynamicSegmentCount.CompareTo(y.DynamicSegmentCount);
        if (dynamicComparison != 0)
        {
            return dynamicComparison;
        }

        // 5. Segment-by-segment comparison: static segments sort before dynamic segments
        var minLength = Math.Min(x.Segments.Length, y.Segments.Length);
        for (var i = 0; i < minLength; i++)
        {
            var xType = x.Segments[i].SegmentType;
            var yType = y.Segments[i].SegmentType;
            if (xType != yType)
            {
                return GetSegmentTypePriority(xType).CompareTo(GetSegmentTypePriority(yType));
            }
        }

        // 6. Tie-breaker: alphabetical order of raw pattern for determinism
        return string.Compare(x.RawPattern, y.RawPattern, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetNonCatchAllSegmentCount(RoutePattern pattern)
    {
        return pattern.Segments.Count(s => s.SegmentType != RouteSegmentType.CatchAll);
    }

    private static int GetSegmentTypePriority(RouteSegmentType segmentType)
    {
        return segmentType switch
        {
            RouteSegmentType.Static => 0,   // Highest priority
            RouteSegmentType.Dynamic => 1,
            RouteSegmentType.CatchAll => 2, // Lowest priority
            _ => 3
        };
    }
}

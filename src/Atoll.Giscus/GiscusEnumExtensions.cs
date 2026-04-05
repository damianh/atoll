namespace Atoll.Giscus;

/// <summary>
/// Extension methods that convert giscus enum values to the string representations
/// expected by the giscus <c>data-*</c> attributes.
/// </summary>
public static class GiscusEnumExtensions
{
    /// <summary>
    /// Converts a <see cref="GiscusMapping"/> value to the giscus <c>data-mapping</c> attribute string.
    /// </summary>
    /// <param name="mapping">The mapping value to convert.</param>
    /// <returns>The giscus wire-format string for the mapping.</returns>
    public static string ToDataValue(this GiscusMapping mapping) => mapping switch
    {
        GiscusMapping.Pathname => "pathname",
        GiscusMapping.Url => "url",
        GiscusMapping.Title => "title",
        GiscusMapping.OgTitle => "og:title",
        GiscusMapping.Specific => "specific",
        GiscusMapping.Number => "number",
        _ => "pathname",
    };

    /// <summary>
    /// Converts a <see cref="GiscusInputPosition"/> value to the giscus
    /// <c>data-input-position</c> attribute string.
    /// </summary>
    /// <param name="inputPosition">The input position value to convert.</param>
    /// <returns>The giscus wire-format string for the input position.</returns>
    public static string ToDataValue(this GiscusInputPosition inputPosition) => inputPosition switch
    {
        GiscusInputPosition.Top => "top",
        GiscusInputPosition.Bottom => "bottom",
        _ => "bottom",
    };

    /// <summary>
    /// Converts a <see cref="GiscusLoading"/> value to the giscus <c>data-loading</c> attribute string.
    /// </summary>
    /// <param name="loading">The loading value to convert.</param>
    /// <returns>The giscus wire-format string for the loading strategy.</returns>
    public static string ToDataValue(this GiscusLoading loading) => loading switch
    {
        GiscusLoading.Lazy => "lazy",
        GiscusLoading.Eager => "eager",
        _ => "lazy",
    };
}

namespace Atoll.Samples.Blog.Components;

/// <summary>
/// Model for the <c>TagBadge</c> Razor slice.
/// </summary>
/// <param name="Tag">The tag name to render as a badge link.</param>
public sealed record TagBadgeModel(string Tag);

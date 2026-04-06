namespace Atoll.Reef.Components;

/// <summary>
/// Model for the <c>TagCloudTemplate</c> Razor slice.
/// </summary>
/// <param name="Tags">The tags with counts to display.</param>
/// <param name="BaseTrimmed">The trimmed base URL path prefix.</param>
public sealed record TagCloudModel(
    IReadOnlyList<TagCount> Tags,
    string BaseTrimmed);

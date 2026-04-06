namespace Atoll.Reef.Components;

/// <summary>
/// Model for the <c>AuthorCardTemplate</c> Razor slice.
/// </summary>
/// <param name="Name">The author's display name.</param>
/// <param name="AvatarUrl">The URL of the author's avatar image, or <c>null</c>.</param>
/// <param name="Bio">The author's short biography text, or <c>null</c>.</param>
/// <param name="Url">The URL of the author's profile page, or <c>null</c>.</param>
public sealed record AuthorCardModel(
    string Name,
    string? AvatarUrl,
    string? Bio,
    string? Url);

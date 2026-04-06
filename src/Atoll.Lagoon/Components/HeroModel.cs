namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>HeroTemplate</c> Razor slice.
/// </summary>
/// <param name="Title">The hero heading text.</param>
/// <param name="Tagline">Optional subtitle text.</param>
/// <param name="ImageSrc">Optional hero image URL.</param>
/// <param name="ImageAlt">Alt text for the hero image.</param>
/// <param name="Actions">Call-to-action buttons to display.</param>
public sealed record HeroModel(
    string Title,
    string? Tagline,
    string? ImageSrc,
    string ImageAlt,
    IReadOnlyList<HeroAction> Actions);

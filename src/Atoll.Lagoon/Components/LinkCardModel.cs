namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>LinkCardTemplate</c> Razor slice.
/// </summary>
/// <param name="Title">The card title text.</param>
/// <param name="Href">The URL the card links to.</param>
/// <param name="Description">Optional description beneath the title.</param>
/// <param name="IconName">Optional icon displayed before the title.</param>
public sealed record LinkCardModel(
    string Title,
    string Href,
    string? Description,
    IconName? IconName);

namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>CardTemplate</c> Razor slice.
/// </summary>
/// <param name="Title">The card title.</param>
/// <param name="IconName">Optional icon displayed alongside the title.</param>
public sealed record CardModel(
    string Title,
    IconName? IconName);

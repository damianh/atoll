namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>AsideTemplate</c> Razor slice.
/// </summary>
/// <param name="VariantClass">The CSS class for the aside variant.</param>
/// <param name="Title">The resolved title text.</param>
/// <param name="IconName">The icon for the aside variant.</param>
public sealed record AsideModel(
    string VariantClass,
    string Title,
    IconName IconName);

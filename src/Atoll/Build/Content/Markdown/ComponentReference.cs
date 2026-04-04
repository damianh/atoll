namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Captures all data needed to render an Atoll component at a placeholder site
/// within rendered Markdown content.
/// </summary>
/// <param name="ComponentType">The component type to instantiate and render.</param>
/// <param name="Props">
/// The props dictionary, with string values parsed from the directive's generic attributes.
/// <see cref="Atoll.Components.ComponentRenderer"/> handles type conversion when binding parameters.
/// </param>
/// <param name="ChildHtml">
/// The HTML rendered from the child blocks inside the directive container,
/// passed as the component's default slot. <c>null</c> when the directive has no children.
/// </param>
public sealed record ComponentReference(
    Type ComponentType,
    IReadOnlyDictionary<string, object?> Props,
    string? ChildHtml);

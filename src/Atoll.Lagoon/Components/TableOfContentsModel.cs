using Atoll.Build.Content.Markdown;
using Atoll.Lagoon.I18n;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Model for the <c>TableOfContentsTemplate</c> Razor slice.
/// </summary>
/// <param name="FilteredHeadings">The headings that passed min/max depth filtering.</param>
/// <param name="MinLevel">The minimum heading level (used as the starting depth for nesting).</param>
/// <param name="Translations">The UI translations.</param>
public sealed record TableOfContentsModel(
    List<MarkdownHeading> FilteredHeadings,
    int MinLevel,
    UiTranslations Translations);

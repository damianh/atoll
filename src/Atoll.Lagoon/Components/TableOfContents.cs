using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Lagoon.I18n;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders an "On this page" table of contents from the headings extracted
/// by <see cref="MarkdownRenderer"/>. Filters headings by configurable min/max depth
/// and renders a nested <c>&lt;nav&gt;</c> / <c>&lt;ul&gt;</c> with anchor links.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>TableOfContentsTemplate.cshtml</c>.
/// </remarks>
public sealed class TableOfContents : AtollComponent
{
    /// <summary>
    /// Gets or sets the headings to include in the table of contents.
    /// Obtained from <see cref="MarkdownRenderResult.Headings"/>.
    /// </summary>
    [Parameter(Required = true)]
    public IReadOnlyList<MarkdownHeading> Headings { get; set; } = [];

    /// <summary>
    /// Gets or sets the minimum heading level to include (inclusive).
    /// Headings shallower than this level are excluded. Default: <c>2</c>.
    /// </summary>
    [Parameter]
    public int MinLevel { get; set; } = 2;

    /// <summary>
    /// Gets or sets the maximum heading level to include (inclusive).
    /// Headings deeper than this level are excluded. Default: <c>3</c>.
    /// </summary>
    [Parameter]
    public int MaxLevel { get; set; } = 3;

    /// <summary>Gets or sets the UI translations. Defaults to English.</summary>
    [Parameter]
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var filtered = Headings
            .Where(h => h.Depth >= MinLevel && h.Depth <= MaxLevel)
            .ToList();

        if (filtered.Count == 0)
        {
            return;
        }

        var model = new TableOfContentsModel(filtered, MinLevel, Translations);

        await ComponentRenderer.RenderSliceAsync<TableOfContentsTemplate, TableOfContentsModel>(
            context.Destination,
            model);
    }
}

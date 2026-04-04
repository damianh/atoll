using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Lagoon.I18n;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders an "On this page" table of contents from the headings extracted
/// by <see cref="MarkdownRenderer"/>. Filters headings by configurable min/max depth
/// and renders a nested <c>&lt;nav&gt;</c> / <c>&lt;ul&gt;</c> with anchor links.
/// </summary>
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
    protected override Task RenderCoreAsync(RenderContext context)
    {
        var filtered = Headings
            .Where(h => h.Depth >= MinLevel && h.Depth <= MaxLevel)
            .ToList();

        if (filtered.Count == 0)
        {
            return Task.CompletedTask;
        }

        WriteHtml($"<nav aria-label=\"{System.Net.WebUtility.HtmlEncode(Translations.TocLabel)}\"><ul>");
        RenderItems(filtered, MinLevel);
        WriteHtml("</ul></nav>");
        return Task.CompletedTask;
    }

    private void RenderItems(List<MarkdownHeading> headings, int currentDepth)
    {
        var i = 0;
        while (i < headings.Count)
        {
            var heading = headings[i];

            if (heading.Depth < currentDepth)
            {
                // Heading is shallower — caller should handle it.
                return;
            }

            if (heading.Depth == currentDepth)
            {
                var anchor = heading.Id is not null ? $"#{heading.Id}" : "";
                WriteHtml("<li>");
                WriteHtml($"<a href=\"{System.Net.WebUtility.HtmlEncode(anchor)}\">");
                WriteText(heading.Text);
                WriteHtml("</a>");

                // Check if next headings are deeper (children).
                var children = new List<MarkdownHeading>();
                var j = i + 1;
                while (j < headings.Count && headings[j].Depth > currentDepth)
                {
                    children.Add(headings[j]);
                    j++;
                }

                if (children.Count > 0)
                {
                    WriteHtml("<ul>");
                    RenderItems(children, currentDepth + 1);
                    WriteHtml("</ul>");
                }

                WriteHtml("</li>");
                i = j; // skip past the children
            }
            else
            {
                // heading.Depth > currentDepth — shouldn't happen in a well-formed list,
                // but handle gracefully by treating as same level.
                i++;
            }
        }
    }
}

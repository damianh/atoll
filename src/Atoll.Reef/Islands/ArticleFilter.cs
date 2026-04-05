using Atoll.Components;
using Atoll.Islands;
using Atoll.Reef.Configuration;

namespace Atoll.Reef.Islands;

/// <summary>
/// A client-side article filter island that renders a filter bar with tag pills and an
/// author dropdown. Filters are applied by toggling visibility of article cards via
/// <c>data-tags</c> and <c>data-author</c> attributes. Hydrates on idle.
/// </summary>
[ClientIdle]
public sealed class ArticleFilter : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-reef-article-filter.js";

    /// <summary>Gets or sets the list of tag names to show as filter pills.</summary>
    [Parameter]
    public IReadOnlyList<string> Tags { get; set; } = [];

    /// <summary>Gets or sets the list of author names to show in the author dropdown.</summary>
    [Parameter]
    public IReadOnlyList<string> Authors { get; set; } = [];

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"article-filter\" data-filter-root>");
        WriteHtml("<div class=\"article-filter__tags\" role=\"group\" aria-label=\"Filter by tag\">");
        WriteHtml("<button class=\"tag-pill tag-pill--active\" data-filter-tag=\"\" aria-pressed=\"true\">All</button>");
        foreach (var tag in Tags)
        {
            var encoded = System.Net.WebUtility.HtmlEncode(tag);
            WriteHtml($"<button class=\"tag-pill\" data-filter-tag=\"{encoded}\" aria-pressed=\"false\">{encoded}</button>");
        }
        WriteHtml("</div>");

        if (Authors.Count > 0)
        {
            WriteHtml("<div class=\"article-filter__authors\">");
            WriteHtml("<label for=\"article-filter-author\" class=\"article-filter__author-label\">Author</label>");
            WriteHtml("<select id=\"article-filter-author\" class=\"article-filter__author-select\" data-filter-author>");
            WriteHtml("<option value=\"\">All authors</option>");
            foreach (var author in Authors)
            {
                var encoded = System.Net.WebUtility.HtmlEncode(author);
                WriteHtml($"<option value=\"{encoded}\">{encoded}</option>");
            }
            WriteHtml("</select>");
            WriteHtml("</div>");
        }

        WriteHtml("</div>");
        return Task.CompletedTask;
    }
}

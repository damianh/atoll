using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using Docs.Layouts;

namespace Docs.Pages;

/// <summary>
/// The individual documentation page. Renders a Markdown doc entry identified
/// by the URL slug and wraps it in a prose layout with the sidebar.
/// Route: /docs/[slug]
/// </summary>
[Layout(typeof(SiteLayout))]
[PageRoute("/docs/[slug]")]
public sealed class DocsPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    /// <summary>
    /// Gets or sets the doc slug from the URL parameter.
    /// </summary>
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    /// <summary>
    /// Gets or sets the collection query used to load documentation entries.
    /// </summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <summary>Gets or sets the page title for the layout &lt;title&gt; and TOC.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <summary>Gets or sets the headings extracted from the rendered Markdown for the TOC.</summary>
    [Parameter]
    public IReadOnlyList<MarkdownHeading> Headings { get; set; } = [];

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var docs = Query.GetCollection<DocSchema>("docs");

        var paths = docs
            .Select(entry => new StaticPath(
                new Dictionary<string, string> { ["slug"] = entry.Slug }))
            .ToList();

        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var entry = Query.GetEntry<DocSchema>("docs", Slug);

        if (entry is null)
        {
            WriteHtml("<h1>Page Not Found</h1><p>The requested documentation page could not be found.</p>");
            return;
        }

        var rendered = Query.Render(entry);

        // Expose metadata so the layout (SiteLayout → DocsLayout) can consume it
        PageTitle = entry.Data.Title;
        PageDescription = entry.Data.Description;
        Headings = rendered.Headings;

        // The addon DocsLayout wraps content in <article class="docs-article prose">,
        // so render the content directly without extra wrappers.
        var contentComponent = ContentComponent.FromRenderedContent(rendered);
        await RenderAsync(contentComponent.ToRenderFragment());
    }
}

using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Routing;
using Docs.Layouts;

namespace Docs.Pages;

/// <summary>
/// The individual documentation page. Renders a Markdown doc entry identified
/// by the URL slug and wraps it in a prose layout with the sidebar.
/// Route: /[...slug]
/// </summary>
[Layout(typeof(SiteLayout))]
[PageRoute("/[...slug]")]
public sealed class DocsPage : AtollComponent, IAtollPage, IStaticPathsProvider, IPageStatusCodeProvider
{
    /// <summary>
    /// The reserved slug for the custom 404 page. A content file at
    /// <c>Content/docs/404.md</c> is rendered when a requested slug is not found.
    /// This slug is excluded from static-path generation, sidebar, and search index.
    /// </summary>
    internal const string NotFoundSlug = "404";

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
    public int ResponseStatusCode { get; private set; } = 200;

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var docs = Query.GetCollection<DocSchema>("docs");

        var paths = docs
            .Where(entry => entry.Slug != NotFoundSlug)
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
            ResponseStatusCode = 404;

            // Try to load a custom 404 page from Content/docs/404.md
            var notFoundEntry = Query.GetEntry<DocSchema>("docs", NotFoundSlug);
            if (notFoundEntry is not null)
            {
                var rendered = Query.Render(notFoundEntry);
                PageTitle = notFoundEntry.Data.Title;
                PageDescription = notFoundEntry.Data.Description;
                Headings = rendered.Headings;

                var notFoundContent = ContentComponent.FromRenderedContent(rendered);
                await notFoundContent.RenderAsync(context);
            }
            else
            {
                // Styled default fallback — rendered within the layout
                PageTitle = "Page Not Found";
                WriteHtml("""
                    <div class="not-found">
                      <h1>Page Not Found</h1>
                      <p>Sorry, we couldn&rsquo;t find the page you&rsquo;re looking for.
                         It may have been moved or removed.</p>
                      <p><a href="/">Return to the documentation home</a></p>
                    </div>
                    """);
            }

            return;
        }

        var renderedEntry = Query.Render(entry);

        // Expose metadata so the layout (SiteLayout → DocsLayout) can consume it
        PageTitle = entry.Data.Title;
        PageDescription = entry.Data.Description;
        Headings = renderedEntry.Headings;

        // The addon DocsLayout wraps content in <article class="docs-article prose">,
        // so render the content directly without extra wrappers.
        // Use RenderAsync on the ContentComponent directly (not ToRenderFragment) so that
        // embedded component directives (:::aside, :::card-grid, etc.) and <PascalCaseName>
        // tags are resolved from the fragment list rather than left as placeholder comments.
        var contentComponent = ContentComponent.FromRenderedContent(renderedEntry);
        await contentComponent.RenderAsync(context);
    }
}

using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Reef.Components;
using Atoll.Routing;
using Atoll.Slots;
using Docs.Layouts;

namespace Docs.Pages;

/// <summary>
/// Stub tag page for the Reef live demo. Lists all sample articles that carry
/// a given tag, so that tag pill links from the demo pages navigate to a real
/// page instead of a 404.
/// Route: /docs/reef/live-demo/tag/[tag]
/// </summary>
[Layout(typeof(SiteLayout))]
[PageRoute("/docs/reef/live-demo/tag/[tag]")]
public sealed class ReefLiveDemoTagPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    private const string BasePath = "/docs/reef/live-demo";

    /// <summary>Gets or sets the tag slug from the route parameter.</summary>
    [Parameter(Required = true)]
    public string Tag { get; set; } = "";

    /// <summary>Gets or sets the collection query (required by SiteLayout).</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <summary>Gets or sets the page title shown in the layout.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page description for the meta tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var allTags = ReefLiveDemoPage.SampleArticles
            .SelectMany(a => a.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var paths = allTags
            .Select(tag => new StaticPath(new Dictionary<string, string> { ["tag"] = tag.ToLowerInvariant() }))
            .ToList();

        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        PageTitle = $"Tag: {Tag}";
        PageDescription = $"Articles tagged with \"{Tag}\" in the Reef live demo.";

        WriteHtml("<div class=\"reef-demo\">");

        WriteHtml("<h1>Tag: ");
        WriteText(Tag);
        WriteHtml("</h1>");

        WriteHtml($"<p style=\"margin-bottom: 1.5rem;\"><a href=\"{BasePath}\">&larr; Back to Reef Live Demo</a></p>");

        var matchingArticles = ReefLiveDemoPage.SampleArticles
            .Where(a => a.Tags.Any(t => string.Equals(t, Tag, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(a => a.PubDate)
            .ToList();

        if (matchingArticles.Count == 0)
        {
            WriteHtml("<p>No articles found with this tag.</p>");
            WriteHtml("</div>");
            return;
        }

        // Render matching articles as a list
        var listFragment = ComponentRenderer.ToFragment<ArticleList>(new Dictionary<string, object?>
        {
            [nameof(ArticleList.Items)] = (IReadOnlyList<ArticleListItem>)matchingArticles,
            [nameof(ArticleList.BasePath)] = BasePath,
        });
        await RenderAsync(listFragment);

        WriteHtml("</div>");
    }
}

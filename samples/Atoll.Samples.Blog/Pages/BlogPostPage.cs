using Atoll.Content.Collections;
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using Atoll.Samples.Blog.Components;
using Atoll.Samples.Blog.Layouts;

namespace Atoll.Samples.Blog.Pages;

/// <summary>
/// The individual blog post page. Displays the rendered Markdown content
/// with post metadata (title, date, author, tags).
/// Route: /blog/[slug]
/// </summary>
[Layout(typeof(BlogLayout))]
[PageRoute("/blog/[slug]")]
public sealed class BlogPostPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    /// <summary>
    /// Gets or sets the post slug from the URL parameter.
    /// </summary>
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    /// <summary>
    /// Gets or sets the collection query for loading blog posts.
    /// </summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
    {
        var posts = Query.GetCollection<BlogPostSchema>("blog",
            entry => !entry.Data.Draft);

        var paths = posts.Select(post =>
            new StaticPath(
                new Dictionary<string, string> { ["slug"] = post.Slug }))
            .ToList();

        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var entry = Query.GetEntry<BlogPostSchema>("blog", Slug);

        if (entry is null)
        {
            WriteHtml("<h1>Post Not Found</h1><p>The requested blog post could not be found.</p>");
            return;
        }

        var rendered = Query.Render(entry);

        // Post header
        WriteHtml("<article>");
        WriteHtml("<header style=\"margin-bottom: 2rem;\">");
        WriteHtml("<h1 style=\"font-size: 2rem; margin-bottom: 0.5rem;\">");
        WriteText(entry.Data.Title);
        WriteHtml("</h1>");

        WriteHtml("<div style=\"color: var(--color-muted); font-size: 0.875rem; display: flex; gap: 1rem; flex-wrap: wrap;\">");

        WriteHtml("<time>");
        WriteText(entry.Data.PubDate.ToString("MMMM d, yyyy"));
        WriteHtml("</time>");

        if (!string.IsNullOrEmpty(entry.Data.Author))
        {
            WriteHtml("<span>by ");
            WriteText(entry.Data.Author);
            WriteHtml("</span>");
        }

        WriteHtml("</div>");

        // Tags
        var tags = entry.Data.GetTags();
        if (tags.Length > 0)
        {
            WriteHtml("<div style=\"margin-top: 0.75rem; display: flex; gap: 0.5rem; flex-wrap: wrap;\">");
            foreach (var tag in tags)
            {
                var badgeProps = new Dictionary<string, object?> { ["Tag"] = tag };
                var badgeFragment = ComponentRenderer.ToFragment<TagBadge>(badgeProps);
                await RenderAsync(badgeFragment);
            }

            WriteHtml("</div>");
        }

        WriteHtml("</header>");

        // Rendered Markdown content
        WriteHtml("<div class=\"prose\" style=\"line-height: 1.8;\">");
        var contentComponent = ContentComponent.FromRenderedContent(rendered);
        await RenderAsync(contentComponent.ToRenderFragment());
        WriteHtml("</div>");

        WriteHtml("</article>");
    }
}

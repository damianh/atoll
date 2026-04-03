using Atoll.Content.Collections;
using Atoll.Core.Components;
using Atoll.Routing;
using Atoll.Samples.Blog.Components;
using Atoll.Samples.Blog.Layouts;

namespace Atoll.Samples.Blog.Pages;

/// <summary>
/// The tags index page. Displays all unique tags across all blog posts.
/// </summary>
[Layout(typeof(BlogLayout))]
[PageRoute("/tags")]
public sealed class TagsIndexPage : AtollComponent, IAtollPage
{
    /// <summary>
    /// Gets or sets the collection query for loading blog posts.
    /// </summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1 style=\"margin-bottom: 1.5rem;\">All Tags</h1>");

        var posts = Query.GetCollection<BlogPostSchema>("blog",
            entry => !entry.Data.Draft);

        // Collect all unique tags with counts
        var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var post in posts)
        {
            foreach (var tag in post.Data.GetTags())
            {
                tagCounts.TryGetValue(tag, out var count);
                tagCounts[tag] = count + 1;
            }
        }

        if (tagCounts.Count == 0)
        {
            WriteHtml("<p>No tags found.</p>");
            return;
        }

        WriteHtml("<div style=\"display: flex; gap: 0.5rem; flex-wrap: wrap;\">");

        foreach (var (tag, count) in tagCounts.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
        {
            var badgeProps = new Dictionary<string, object?> { ["Tag"] = tag };
            var badgeFragment = ComponentRenderer.ToFragment<TagBadge>(badgeProps);
            await RenderAsync(badgeFragment);
            WriteHtml("<span style=\"color: var(--color-muted); font-size: 0.75rem; align-self: center;\">(");
            WriteText(count.ToString());
            WriteHtml(")</span>");
        }

        WriteHtml("</div>");
    }
}

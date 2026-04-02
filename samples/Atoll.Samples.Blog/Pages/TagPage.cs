using Atoll.Content.Collections;
using Atoll.Core.Components;
using Atoll.Routing;
using Atoll.Samples.Blog.Components;
using Atoll.Samples.Blog.Layouts;

namespace Atoll.Samples.Blog.Pages;

/// <summary>
/// The tag-filtered blog listing page. Displays posts that have a specific tag.
/// Route: /tags/[tag]
/// </summary>
[Layout(typeof(BlogLayout))]
public sealed class TagPage : AtollComponent, IAtollPage, IStaticPathsProvider
{
    /// <summary>
    /// Gets or sets the tag name from the URL parameter.
    /// </summary>
    [Parameter(Required = true)]
    public string Tag { get; set; } = "";

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

        var allTags = posts
            .SelectMany(p => p.Data.GetTags())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var paths = allTags.Select(tag =>
            new StaticPath(
                new Dictionary<string, string> { ["tag"] = tag.ToLowerInvariant() }))
            .ToList();

        return Task.FromResult<IReadOnlyList<StaticPath>>(paths);
    }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1 style=\"margin-bottom: 0.5rem;\">Posts tagged: ");
        WriteText(Tag);
        WriteHtml("</h1>");
        WriteHtml("<p style=\"margin-bottom: 1.5rem;\"><a href=\"/tags\">&larr; All tags</a></p>");

        var posts = Query.GetCollection<BlogPostSchema>("blog",
            entry => !entry.Data.Draft &&
                     entry.Data.GetTags().Any(t =>
                         string.Equals(t, Tag, StringComparison.OrdinalIgnoreCase)));

        var sorted = posts
            .OrderByDescending(p => p.Data.PubDate)
            .ToList();

        if (sorted.Count == 0)
        {
            WriteHtml("<p>No posts found with this tag.</p>");
            return;
        }

        foreach (var post in sorted)
        {
            var postCardProps = new Dictionary<string, object?>
            {
                ["Title"] = post.Data.Title,
                ["Slug"] = post.Slug,
                ["Description"] = post.Data.Description,
                ["Date"] = post.Data.PubDate.ToString("MMMM d, yyyy"),
                ["Tags"] = post.Data.Tags,
            };
            var fragment = ComponentRenderer.ToFragment<PostCard>(postCardProps);
            await RenderAsync(fragment);
        }
    }
}

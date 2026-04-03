using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using Atoll.Samples.Blog.Components;
using Atoll.Samples.Blog.Layouts;

namespace Atoll.Samples.Blog.Pages;

/// <summary>
/// The blog listing page. Displays all published blog posts as cards, sorted by date.
/// Uses a <see cref="CollectionQuery"/> to load content from the blog collection.
/// </summary>
[Layout(typeof(BlogLayout))]
[PageRoute("/blog")]
public sealed class BlogIndexPage : AtollComponent, IAtollPage
{
    /// <summary>
    /// Gets or sets the collection query for loading blog posts.
    /// </summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1 style=\"margin-bottom: 1.5rem;\">Blog Posts</h1>");

        var posts = Query.GetCollection<BlogPostSchema>("blog",
            entry => !entry.Data.Draft);

        // Sort by date descending (newest first)
        var sorted = posts
            .OrderByDescending(p => p.Data.PubDate)
            .ToList();

        if (sorted.Count == 0)
        {
            WriteHtml("<p>No posts yet. Check back soon!</p>");
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

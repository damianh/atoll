using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Routing;

namespace Atoll.Samples.Monorepo.Pages;

/// <summary>
/// Index page listing both blog posts and external docs to demonstrate
/// content from multiple source directories.
/// </summary>
[PageRoute("/")]
public sealed class IndexPage : AtollComponent, IAtollPage
{
    /// <summary>Gets or sets the collection query for loading content.</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<html><body>");
        WriteHtml("<h1>Monorepo Sample</h1>");

        // Blog posts from the standard Content/blog/ directory
        WriteHtml("<h2>Blog Posts</h2>");
        WriteHtml("<ul>");
        var posts = Query.GetCollection<BlogPostSchema>("blog",
            entry => !entry.Data.Draft);
        foreach (var post in posts.OrderByDescending(p => p.Data.PubDate))
        {
            WriteHtml($"<li>{post.Data.Title} — {post.Data.PubDate:yyyy-MM-dd}</li>");
        }
        WriteHtml("</ul>");

        // Docs from the external ExternalDocs/weather-api/ directory
        WriteHtml("<h2>Weather API Docs</h2>");
        WriteHtml("<ul>");
        var docs = Query.GetCollection<DocSchema>("weather-api-docs");
        foreach (var doc in docs.OrderBy(d => d.Data.Order))
        {
            WriteHtml($"<li>{doc.Data.Title}</li>");
        }
        WriteHtml("</ul>");

        WriteHtml("</body></html>");
        await Task.CompletedTask;
    }
}

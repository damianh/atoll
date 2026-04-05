using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Reef.Configuration;
using Atoll.Routing;

namespace Atoll.Samples.Articles.Pages;

/// <summary>
/// Displays all unique tags across all articles.
/// Route: /articles/tags
/// </summary>
[Layout(typeof(ArticlesLayout))]
[PageRoute("/articles/tags")]
public sealed class TagsIndexPage : AtollComponent, IAtollPage
{
    /// <summary>Gets or sets the collection query for loading articles.</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        var basePath = ArticlesConfig.Current.BasePath;

        var entries = Query.GetCollection<ArticleSchema>("articles",
            entry => !entry.Data.Draft);

        var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            foreach (var tag in entry.Data.GetTags())
            {
                tagCounts.TryGetValue(tag, out var count);
                tagCounts[tag] = count + 1;
            }
        }

        if (tagCounts.Count == 0)
        {
            WriteHtml("<p>No tags found.</p>");
            return Task.CompletedTask;
        }

        WriteHtml("<nav class=\"tag-cloud\" aria-label=\"Tags\">");
        foreach (var (tag, count) in tagCounts.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
        {
            WriteHtml("<a href=\"");
            WriteHtml(System.Net.WebUtility.HtmlEncode(basePath + "/tag/" + tag.ToLowerInvariant()));
            WriteHtml("\" class=\"tag-pill\">");
            WriteText(tag);
            WriteHtml(" <span class=\"tag-count\">(");
            WriteText(count.ToString());
            WriteHtml(")</span></a>");
        }

        WriteHtml("</nav>");
        return Task.CompletedTask;
    }
}

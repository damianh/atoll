using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a compact vertical list of article entries, each showing title, date, and description.
/// </summary>
public sealed class ArticleList : AtollComponent
{
    /// <summary>Gets or sets the list of article items to display.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<ArticleListItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the base URL path prefix for the articles site (e.g. <c>"/blog"</c>).
    /// </summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"article-list\">");

        foreach (var item in Items)
        {
            var basePath = BasePath.TrimEnd('/');
            var href = $"{basePath}/{item.Slug.TrimStart('/')}";

            WriteHtml("<article class=\"article-list-item\">");

            WriteHtml("<div class=\"article-list-item-header\">");

            WriteHtml("<h3 class=\"article-list-item-title\">");
            WriteHtml("<a href=\"");
            WriteHtml(HtmlEncode(href));
            WriteHtml("\">");
            WriteText(item.Title);
            WriteHtml("</a>");
            WriteHtml("</h3>");

            WriteHtml("<time class=\"article-list-item-date\" datetime=\"");
            WriteHtml(item.PubDate.ToString("yyyy-MM-dd"));
            WriteHtml("\">");
            WriteText(item.PubDate.ToString("MMM d, yyyy"));
            WriteHtml("</time>");

            WriteHtml("</div>");

            if (!string.IsNullOrEmpty(item.Description))
            {
                WriteHtml("<p class=\"article-list-item-description\">");
                WriteText(item.Description);
                WriteHtml("</p>");
            }

            WriteHtml("</article>");
        }

        WriteHtml("</div>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

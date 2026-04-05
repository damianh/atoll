using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders articles in a tabular view with configurable columns: title, date, author, tags,
/// and reading time.
/// </summary>
public sealed class ArticleTable : AtollComponent
{
    /// <summary>Gets or sets the list of article items to display.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<ArticleListItem> Items { get; set; } = [];

    /// <summary>Gets or sets whether to show the Author column. Defaults to <c>true</c>.</summary>
    [Parameter]
    public bool ShowAuthor { get; set; } = true;

    /// <summary>Gets or sets whether to show the Tags column. Defaults to <c>true</c>.</summary>
    [Parameter]
    public bool ShowTags { get; set; } = true;

    /// <summary>Gets or sets whether to show the Reading Time column. Defaults to <c>true</c>.</summary>
    [Parameter]
    public bool ShowReadingTime { get; set; } = true;

    /// <summary>
    /// Gets or sets the base URL path prefix for the articles site (e.g. <c>"/blog"</c>).
    /// </summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<table class=\"article-table\">");
        WriteHtml("<thead>");
        WriteHtml("<tr>");
        WriteHtml("<th scope=\"col\">Title</th>");
        WriteHtml("<th scope=\"col\">Date</th>");
        if (ShowAuthor) WriteHtml("<th scope=\"col\">Author</th>");
        if (ShowTags) WriteHtml("<th scope=\"col\">Tags</th>");
        if (ShowReadingTime) WriteHtml("<th scope=\"col\">Reading Time</th>");
        WriteHtml("</tr>");
        WriteHtml("</thead>");
        WriteHtml("<tbody>");

        foreach (var item in Items)
        {
            var basePath = BasePath.TrimEnd('/');
            var href = HtmlEncode($"{basePath}/{item.Slug.TrimStart('/')}");

            WriteHtml("<tr>");

            WriteHtml("<td class=\"article-table__title\">");
            WriteHtml("<a href=\"");
            WriteHtml(href);
            WriteHtml("\">");
            WriteText(item.Title);
            WriteHtml("</a>");
            WriteHtml("</td>");

            WriteHtml("<td class=\"article-table__date\">");
            WriteHtml("<time datetime=\"");
            WriteHtml(item.PubDate.ToString("yyyy-MM-dd"));
            WriteHtml("\">");
            WriteText(item.PubDate.ToString("MMM d, yyyy"));
            WriteHtml("</time>");
            WriteHtml("</td>");

            if (ShowAuthor)
            {
                WriteHtml("<td class=\"article-table__author\">");
                if (!string.IsNullOrEmpty(item.Author)) WriteText(item.Author);
                WriteHtml("</td>");
            }

            if (ShowTags)
            {
                WriteHtml("<td class=\"article-table__tags\">");
                foreach (var tag in item.Tags)
                {
                    WriteHtml("<span class=\"tag-pill\">");
                    WriteText(tag);
                    WriteHtml("</span>");
                }
                WriteHtml("</td>");
            }

            if (ShowReadingTime)
            {
                WriteHtml("<td class=\"article-table__reading-time\">");
                if (item.ReadingTimeMinutes.HasValue)
                {
                    WriteText($"{item.ReadingTimeMinutes} min");
                }
                WriteHtml("</td>");
            }

            WriteHtml("</tr>");
        }

        WriteHtml("</tbody>");
        WriteHtml("</table>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

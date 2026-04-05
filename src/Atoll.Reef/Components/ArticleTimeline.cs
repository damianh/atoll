using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a chronological timeline of articles grouped by year (and optionally by month).
/// Each article appears as a timeline entry with a date marker and title link.
/// </summary>
public sealed class ArticleTimeline : AtollComponent
{
    /// <summary>Gets or sets the list of article items to display in the timeline.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<ArticleListItem> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets whether to group entries by month within each year.
    /// When <c>false</c> (default), entries are grouped by year only.
    /// </summary>
    [Parameter]
    public bool GroupByMonth { get; set; }

    /// <summary>
    /// Gets or sets the base URL path prefix for the articles site (e.g. <c>"/blog"</c>).
    /// </summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"article-timeline\">");

        if (GroupByMonth)
        {
            RenderGroupedByMonth();
        }
        else
        {
            RenderGroupedByYear();
        }

        WriteHtml("</div>");
        return Task.CompletedTask;
    }

    private void RenderGroupedByYear()
    {
        var byYear = Items
            .OrderByDescending(i => i.PubDate)
            .GroupBy(i => i.PubDate.Year)
            .OrderByDescending(g => g.Key);

        foreach (var yearGroup in byYear)
        {
            WriteHtml("<section class=\"timeline-year\">");
            WriteHtml($"<h2 class=\"timeline-year__heading\">{yearGroup.Key}</h2>");
            WriteHtml("<ol class=\"timeline-entries\">");

            foreach (var item in yearGroup)
            {
                WriteTimelineEntry(item);
            }

            WriteHtml("</ol>");
            WriteHtml("</section>");
        }
    }

    private void RenderGroupedByMonth()
    {
        var byYearMonth = Items
            .OrderByDescending(i => i.PubDate)
            .GroupBy(i => new { i.PubDate.Year, i.PubDate.Month })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Month);

        foreach (var group in byYearMonth)
        {
            var monthName = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM yyyy");
            WriteHtml("<section class=\"timeline-year\">");
            WriteHtml($"<h2 class=\"timeline-year__heading\">");
            WriteText(monthName);
            WriteHtml("</h2>");
            WriteHtml("<ol class=\"timeline-entries\">");

            foreach (var item in group)
            {
                WriteTimelineEntry(item);
            }

            WriteHtml("</ol>");
            WriteHtml("</section>");
        }
    }

    private void WriteTimelineEntry(ArticleListItem item)
    {
        var basePath = BasePath.TrimEnd('/');
        var href = HtmlEncode($"{basePath}/{item.Slug.TrimStart('/')}");

        WriteHtml("<li class=\"timeline-entry\">");
        WriteHtml("<time class=\"timeline-entry__date\" datetime=\"");
        WriteHtml(item.PubDate.ToString("yyyy-MM-dd"));
        WriteHtml("\">");
        WriteText(item.PubDate.ToString("MMM d"));
        WriteHtml("</time>");
        WriteHtml("<a class=\"timeline-entry__title\" href=\"");
        WriteHtml(href);
        WriteHtml("\">");
        WriteText(item.Title);
        WriteHtml("</a>");
        WriteHtml("</li>");
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

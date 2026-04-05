using Atoll.Components;
using Atoll.Reef.Navigation;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders page number navigation for listing pages. Shows previous/next links and numbered
/// page buttons with ellipsis truncation for large page counts.
/// </summary>
public sealed class Pagination : AtollComponent
{
    private const int WindowSize = 2; // pages on each side of current page

    /// <summary>Gets or sets the pagination state. Required.</summary>
    [Parameter(Required = true)]
    public PaginationInfo Info { get; set; } = null!;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        if (Info.TotalPages <= 1)
        {
            return Task.CompletedTask;
        }

        WriteHtml("<nav class=\"pagination\" aria-label=\"Page navigation\">");
        WriteHtml("<ul class=\"pagination-list\">");

        // Previous link
        WriteHtml("<li class=\"pagination-item\">");
        if (Info.HasPrevious)
        {
            WriteHtml("<a class=\"pagination-link pagination-prev\" href=\"");
            WriteHtml(HtmlEncode(Info.GetPageUrl(Info.CurrentPage - 1)));
            WriteHtml("\" aria-label=\"Previous page\">&laquo;</a>");
        }
        else
        {
            WriteHtml("<span class=\"pagination-link pagination-prev pagination-disabled\" aria-disabled=\"true\">&laquo;</span>");
        }

        WriteHtml("</li>");

        // Page number buttons (with ellipsis)
        var pages = GetPageNumbers(Info.CurrentPage, Info.TotalPages);
        var prev = -1;
        foreach (var page in pages)
        {
            if (prev != -1 && page - prev > 1)
            {
                // Ellipsis gap
                WriteHtml("<li class=\"pagination-item\"><span class=\"pagination-ellipsis\">&hellip;</span></li>");
            }

            WriteHtml("<li class=\"pagination-item\">");
            if (page == Info.CurrentPage)
            {
                WriteHtml("<span class=\"pagination-link pagination-current\" aria-current=\"page\">");
                WriteHtml(page.ToString());
                WriteHtml("</span>");
            }
            else
            {
                WriteHtml("<a class=\"pagination-link\" href=\"");
                WriteHtml(HtmlEncode(Info.GetPageUrl(page)));
                WriteHtml("\">");
                WriteHtml(page.ToString());
                WriteHtml("</a>");
            }

            WriteHtml("</li>");
            prev = page;
        }

        // Next link
        WriteHtml("<li class=\"pagination-item\">");
        if (Info.HasNext)
        {
            WriteHtml("<a class=\"pagination-link pagination-next\" href=\"");
            WriteHtml(HtmlEncode(Info.GetPageUrl(Info.CurrentPage + 1)));
            WriteHtml("\" aria-label=\"Next page\">&raquo;</a>");
        }
        else
        {
            WriteHtml("<span class=\"pagination-link pagination-next pagination-disabled\" aria-disabled=\"true\">&raquo;</span>");
        }

        WriteHtml("</li>");
        WriteHtml("</ul>");
        WriteHtml("</nav>");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the set of page numbers to display, always including the first and last pages
    /// plus a window of <see cref="WindowSize"/> pages around the current page.
    /// </summary>
    private static IEnumerable<int> GetPageNumbers(int current, int total)
    {
        var pages = new SortedSet<int> { 1, total };

        for (var i = current - WindowSize; i <= current + WindowSize; i++)
        {
            if (i >= 1 && i <= total)
            {
                pages.Add(i);
            }
        }

        return pages;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

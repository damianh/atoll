using Atoll.Components;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders previous/next navigation links at the bottom of documentation pages.
/// </summary>
public sealed class Pagination : AtollComponent
{
    /// <summary>Gets or sets the link to the previous page, or <c>null</c> if this is the first page.</summary>
    [Parameter]
    public PaginationLink? Previous { get; set; }

    /// <summary>Gets or sets the link to the next page, or <c>null</c> if this is the last page.</summary>
    [Parameter]
    public PaginationLink? Next { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        if (Previous is null && Next is null)
        {
            return Task.CompletedTask;
        }

        WriteHtml("<nav aria-label=\"Pagination\">");

        if (Previous is not null)
        {
            WriteHtml($"<a href=\"{HtmlEncode(Previous.Href)}\" rel=\"prev\">");
            WriteHtml("<span class=\"pagination-direction\">Previous</span>");
            WriteHtml("<span class=\"pagination-label\">");
            WriteText(Previous.Label);
            WriteHtml("</span>");
            WriteHtml("</a>");
        }

        if (Next is not null)
        {
            WriteHtml($"<a href=\"{HtmlEncode(Next.Href)}\" rel=\"next\">");
            WriteHtml("<span class=\"pagination-direction\">Next</span>");
            WriteHtml("<span class=\"pagination-label\">");
            WriteText(Next.Label);
            WriteHtml("</span>");
            WriteHtml("</a>");
        }

        WriteHtml("</nav>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

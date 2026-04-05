using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a previous/next navigation bar between individual articles.
/// Links are only rendered for non-<see langword="null"/> values; the component
/// renders nothing when both <see cref="Previous"/> and <see cref="Next"/> are
/// <see langword="null"/>.
/// </summary>
public sealed class ArticleNav : AtollComponent
{
    /// <summary>Gets or sets the link to the previous article, or <see langword="null"/> if none.</summary>
    [Parameter]
    public ArticleNavLink? Previous { get; set; }

    /// <summary>Gets or sets the link to the next article, or <see langword="null"/> if none.</summary>
    [Parameter]
    public ArticleNavLink? Next { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        if (Previous is null && Next is null)
        {
            return Task.CompletedTask;
        }

        WriteHtml("<nav class=\"article-nav\" aria-label=\"Article navigation\">");

        if (Previous is not null)
        {
            WriteHtml("<div class=\"article-nav-prev\">");
            WriteHtml("<span class=\"article-nav-label\">&larr; Previous</span>");
            WriteHtml("<a class=\"article-nav-link\" href=\"");
            WriteHtml(HtmlEncode(Previous.Href));
            WriteHtml("\">");
            WriteHtml(HtmlEncode(Previous.Title));
            WriteHtml("</a>");
            WriteHtml("</div>");
        }

        if (Next is not null)
        {
            WriteHtml("<div class=\"article-nav-next\">");
            WriteHtml("<span class=\"article-nav-label\">Next &rarr;</span>");
            WriteHtml("<a class=\"article-nav-link\" href=\"");
            WriteHtml(HtmlEncode(Next.Href));
            WriteHtml("\">");
            WriteHtml(HtmlEncode(Next.Title));
            WriteHtml("</a>");
            WriteHtml("</div>");
        }

        WriteHtml("</nav>");

        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a "Related Articles" section with a heading and a list of article links,
/// typically populated by <c>RelatedArticlesResolver</c> based on shared tags.
/// </summary>
public sealed class RelatedArticles : AtollComponent
{
    /// <summary>Gets or sets the related article links to display.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<ArticleNavLink> Articles { get; set; } = [];

    /// <summary>Gets or sets the section heading. Defaults to <c>"Related Articles"</c>.</summary>
    [Parameter]
    public string Heading { get; set; } = "Related Articles";

    /// <summary>Gets or sets the maximum number of items to render. Defaults to <c>3</c>.</summary>
    [Parameter]
    public int MaxItems { get; set; } = 3;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        var items = Articles.Take(MaxItems).ToList();
        if (items.Count == 0)
        {
            return Task.CompletedTask;
        }

        WriteHtml("<aside class=\"related-articles\">");
        WriteHtml("<h3 class=\"related-articles__heading\">");
        WriteText(Heading);
        WriteHtml("</h3>");
        WriteHtml("<ul class=\"related-articles__list\">");

        foreach (var article in items)
        {
            WriteHtml("<li class=\"related-articles__item\">");
            WriteHtml("<a href=\"");
            WriteHtml(HtmlEncode(article.Href));
            WriteHtml("\">");
            WriteText(article.Title);
            WriteHtml("</a>");
            WriteHtml("</li>");
        }

        WriteHtml("</ul>");
        WriteHtml("</aside>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

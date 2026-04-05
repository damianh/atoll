using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a tag cloud: a navigation region containing tag pills with article counts and links.
/// </summary>
public sealed class TagCloud : AtollComponent
{
    /// <summary>Gets or sets the tags with counts to display.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<TagCount> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the base URL path prefix for the articles site (e.g. <c>"/blog"</c>).
    /// Used to construct <c>/tag/{slug}</c> links.
    /// </summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<nav class=\"tag-cloud\" aria-label=\"Tags\">");

        var baseTrimmed = BasePath.TrimEnd('/');
        foreach (var tag in Tags)
        {
            var href = HtmlEncode($"{baseTrimmed}/tag/{tag.Slug}");
            WriteHtml("<a class=\"tag-pill\" href=\"");
            WriteHtml(href);
            WriteHtml("\">");
            WriteText(tag.Name);
            WriteHtml(" <span class=\"tag-count\">(");
            WriteText(tag.Count.ToString());
            WriteHtml(")</span></a>");
        }

        WriteHtml("</nav>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

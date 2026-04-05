using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders an author bio card showing avatar, name, bio text, and an optional profile link.
/// </summary>
public sealed class AuthorCard : AtollComponent
{
    /// <summary>Gets or sets the author's display name.</summary>
    [Parameter(Required = true)]
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the URL of the author's avatar image. Optional.</summary>
    [Parameter]
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the author's short biography text. Optional.</summary>
    [Parameter]
    public string? Bio { get; set; }

    /// <summary>Gets or sets the URL of the author's profile page. Optional.</summary>
    [Parameter]
    public string? Url { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<aside class=\"author-card\">");

        if (!string.IsNullOrEmpty(AvatarUrl))
        {
            WriteHtml("<img class=\"author-card__avatar\" src=\"");
            WriteHtml(HtmlEncode(AvatarUrl));
            WriteHtml("\" alt=\"");
            WriteHtml(HtmlEncode(Name));
            WriteHtml("\" />");
        }

        WriteHtml("<div class=\"author-card__info\">");

        if (!string.IsNullOrEmpty(Url))
        {
            WriteHtml("<a class=\"author-card__name\" href=\"");
            WriteHtml(HtmlEncode(Url));
            WriteHtml("\">");
            WriteText(Name);
            WriteHtml("</a>");
        }
        else
        {
            WriteHtml("<p class=\"author-card__name\">");
            WriteText(Name);
            WriteHtml("</p>");
        }

        if (!string.IsNullOrEmpty(Bio))
        {
            WriteHtml("<p class=\"author-card__bio\">");
            WriteText(Bio);
            WriteHtml("</p>");
        }

        WriteHtml("</div>");
        WriteHtml("</aside>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A configurable hero section for the docs landing page, supporting a title, tagline,
/// optional image, and call-to-action buttons.
/// </summary>
public sealed class Hero : AtollComponent
{
    /// <summary>Gets or sets the hero title (large heading text).</summary>
    [Parameter(Required = true)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the hero tagline (subtitle text).</summary>
    [Parameter]
    public string? Tagline { get; set; }

    /// <summary>Gets or sets the URL of the hero image. Optional.</summary>
    [Parameter]
    public string? ImageSrc { get; set; }

    /// <summary>Gets or sets the alt text for the hero image.</summary>
    [Parameter]
    public string ImageAlt { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of call-to-action buttons to display.</summary>
    [Parameter]
    public IReadOnlyList<HeroAction> Actions { get; set; } = [];

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<section class=\"hero\">");

        WriteHtml("<div class=\"hero-content\">");

        WriteHtml("<h1 class=\"hero-title\">");
        WriteText(Title);
        WriteHtml("</h1>");

        if (Tagline is not null)
        {
            WriteHtml("<p class=\"hero-tagline\">");
            WriteText(Tagline);
            WriteHtml("</p>");
        }

        if (Actions.Count > 0)
        {
            WriteHtml("<div class=\"hero-actions\">");
            foreach (var action in Actions)
            {
                var variantClass = action.Variant == HeroActionVariant.Primary
                    ? "hero-action hero-action-primary"
                    : "hero-action hero-action-secondary";
                WriteHtml($"<a href=\"{HtmlEncode(action.Href)}\" class=\"{variantClass}\">");
                WriteText(action.Label);
                WriteHtml("</a>");
            }

            WriteHtml("</div>");
        }

        WriteHtml("</div>");

        if (ImageSrc is not null)
        {
            WriteHtml($"<div class=\"hero-image\"><img src=\"{HtmlEncode(ImageSrc)}\" alt=\"{HtmlEncode(ImageAlt)}\"></div>");
        }

        WriteHtml("</section>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

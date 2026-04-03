using Atoll.Components;
using Atoll.Islands;

namespace Atoll.Samples.Portfolio.Islands;

/// <summary>
/// An image gallery island component that loads lazily when scrolled into view.
/// Uses <c>client:visible</c> so the gallery JavaScript (lightbox, navigation)
/// is only loaded when the user scrolls to the gallery section.
/// Demonstrates the <see cref="ClientVisibleAttribute"/> directive.
/// </summary>
[ClientVisible(RootMargin = "200px")]
public sealed class ImageGallery : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/image-gallery.js";

    /// <summary>
    /// Gets or sets the comma-separated list of image URLs to display.
    /// </summary>
    [Parameter]
    public string ImageUrls { get; set; } = "";

    /// <summary>
    /// Gets or sets the comma-separated list of image captions.
    /// </summary>
    [Parameter]
    public string Captions { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"image-gallery\" style=\"display: grid; grid-template-columns: repeat(auto-fill, minmax(16rem, 1fr)); gap: 1rem;\">");

        var urls = string.IsNullOrEmpty(ImageUrls)
            ? []
            : ImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var captions = string.IsNullOrEmpty(Captions)
            ? []
            : Captions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (urls.Length == 0)
        {
            // Render placeholder thumbnails when no URLs provided
            for (var i = 0; i < 6; i++)
            {
                WriteHtml("<div class=\"gallery-item\" style=\"aspect-ratio: 4/3; background: linear-gradient(135deg, var(--color-surface), var(--color-border)); border-radius: 0.5rem; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: transform 0.2s;\">");
                WriteHtml("<span style=\"font-size: 2rem; opacity: 0.3;\">&#128247;</span>");
                WriteHtml("</div>");
            }
        }
        else
        {
            for (var i = 0; i < urls.Length; i++)
            {
                var caption = i < captions.Length ? captions[i] : "";
                WriteHtml("<div class=\"gallery-item\" data-index=\"");
                WriteText(i.ToString());
                WriteHtml("\" style=\"aspect-ratio: 4/3; overflow: hidden; border-radius: 0.5rem; cursor: pointer; transition: transform 0.2s; position: relative;\">");
                WriteHtml("<img src=\"");
                WriteText(urls[i]);
                WriteHtml("\" alt=\"");
                WriteText(caption);
                WriteHtml("\" style=\"width: 100%; height: 100%; object-fit: cover;\" loading=\"lazy\" />");
                if (!string.IsNullOrEmpty(caption))
                {
                    WriteHtml("<div style=\"position: absolute; bottom: 0; left: 0; right: 0; padding: 0.5rem; background: linear-gradient(transparent, rgba(0,0,0,0.7)); color: white; font-size: 0.8125rem;\">");
                    WriteText(caption);
                    WriteHtml("</div>");
                }

                WriteHtml("</div>");
            }
        }

        WriteHtml("</div>");
        WriteHtml("<div id=\"gallery-lightbox\" style=\"display: none; position: fixed; inset: 0; background: rgba(0,0,0,0.9); z-index: 1000; align-items: center; justify-content: center;\"></div>");
        return Task.CompletedTask;
    }
}

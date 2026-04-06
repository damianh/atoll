using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A configurable hero section for the docs landing page, supporting a title, tagline,
/// optional image, and call-to-action buttons.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>HeroTemplate.cshtml</c>.
/// </remarks>
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
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new HeroModel(Title, Tagline, ImageSrc, ImageAlt, Actions);

        await ComponentRenderer.RenderSliceAsync<HeroTemplate, HeroModel>(
            context.Destination,
            model);
    }
}

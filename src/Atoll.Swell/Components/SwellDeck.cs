using Atoll.Components;

namespace Atoll.Swell.Components;

/// <summary>
/// Embeds a Swell slide deck as an inline <c>&lt;iframe&gt;</c> with a fixed aspect-ratio
/// container and a fullscreen breakout button. Use this component in any Atoll page to
/// display a presentation inline.
/// </summary>
/// <example>
/// In a Markdown file or Razor template:
/// <code>
/// &lt;SwellDeck src="/slides/" title="My Talk" /&gt;
/// </code>
/// </example>
public sealed class SwellDeck : AtollComponent
{
    /// <summary>
    /// Gets or sets the URL of the slide deck page to embed. Required.
    /// </summary>
    [Parameter(Required = true)]
    public string Src { get; set; } = null!;

    /// <summary>
    /// Gets or sets the accessible title for the iframe. Defaults to "Slide deck".
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Slide deck";

    /// <summary>
    /// Gets or sets the CSS aspect-ratio value (e.g. "16/9", "4/3"). Defaults to "16/9".
    /// </summary>
    [Parameter]
    public string AspectRatio { get; set; } = "16/9";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new SwellDeckModel(Src, AspectRatio, Title);

        await ComponentRenderer.RenderSliceAsync<SwellDeckTemplate, SwellDeckModel>(
            context.Destination, model);
    }
}

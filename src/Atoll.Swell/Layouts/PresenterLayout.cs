using Atoll.Components;
using Atoll.Swell.Markdown;

namespace Atoll.Swell.Layouts;

/// <summary>
/// Renders the presenter mode HTML page. Opens via <c>window.open()</c> from the navigation
/// island when <kbd>p</kbd> is pressed. Displays current slide, next slide preview,
/// presenter notes, elapsed timer, and wall clock.
/// </summary>
public sealed class PresenterLayout : AtollComponent
{
    /// <summary>Gets or sets the deck-wide configuration. Required.</summary>
    [Parameter(Required = true)]
    public DeckConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the per-slide entries for metadata (notes, config).</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<RenderedSlideEntry> Slides { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new PresenterLayoutModel(Config, Slides, Config.Title);

        await ComponentRenderer.RenderSliceAsync<PresenterLayoutTemplate, PresenterLayoutModel>(
            context.Destination, model);
    }
}

using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using Atoll.Swell.Components;

namespace AtollSwell.Pages;

/// <summary>
/// The main slides page. Route: /
/// Loads the slide deck from Content/slides.md and renders it as a full-page presentation.
/// </summary>
[PageRoute("/")]
public sealed class SlidesPage : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var slidePath = Path.Combine(Directory.GetCurrentDirectory(), "Content", "slides.md");
        var content = await File.ReadAllTextAsync(slidePath);

        var props = new Dictionary<string, object?>
        {
            [nameof(SwellPage.MarkdownContent)] = content,
        };

        await ComponentRenderer.RenderComponentAsync<SwellPage>(context.Destination, props);
    }
}

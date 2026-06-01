using Atoll.Components;

namespace Atoll.Swell.Layouts;

/// <summary>Section divider slide layout — large centered section title, minimal decoration.</summary>
public sealed class SectionSlideLayout : SlideLayoutBase
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        await ComponentRenderer.RenderSliceAsync<SectionSlideLayoutTemplate, SlideLayoutModel>(
            context.Destination, BuildModel(), BuildSlots(context));
    }
}

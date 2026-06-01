using Atoll.Components;

namespace Atoll.Swell.Layouts;

/// <summary>Center slide layout — content is vertically and horizontally centered.</summary>
public sealed class CenterSlideLayout : SlideLayoutBase
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        await ComponentRenderer.RenderSliceAsync<CenterSlideLayoutTemplate, SlideLayoutModel>(
            context.Destination, BuildModel(), BuildSlots(context));
    }
}

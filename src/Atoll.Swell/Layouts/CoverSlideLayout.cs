using Atoll.Components;

namespace Atoll.Swell.Layouts;

/// <summary>Cover slide layout — centered hero with large title. Ideal for the opening slide.</summary>
public sealed class CoverSlideLayout : SlideLayoutBase
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        await ComponentRenderer.RenderSliceAsync<CoverSlideLayoutTemplate, SlideLayoutModel>(
            context.Destination, BuildModel(), BuildSlots(context));
    }
}

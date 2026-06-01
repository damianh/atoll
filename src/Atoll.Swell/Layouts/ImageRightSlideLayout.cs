using Atoll.Components;

namespace Atoll.Swell.Layouts;

/// <summary>Image-right slide layout — content on the left, image on the right.</summary>
public sealed class ImageRightSlideLayout : SlideLayoutBase
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        await ComponentRenderer.RenderSliceAsync<ImageRightSlideLayoutTemplate, SlideLayoutModel>(
            context.Destination, BuildModel(), BuildSlots(context));
    }
}

using Atoll.Components;

namespace Atoll.Swell.Layouts;

/// <summary>Image-left slide layout — image on the left, content on the right.</summary>
public sealed class ImageLeftSlideLayout : SlideLayoutBase
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        await ComponentRenderer.RenderSliceAsync<ImageLeftSlideLayoutTemplate, SlideLayoutModel>(
            context.Destination, BuildModel(), BuildSlots(context));
    }
}

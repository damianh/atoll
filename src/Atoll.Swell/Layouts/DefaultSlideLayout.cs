using Atoll.Components;

namespace Atoll.Swell.Layouts;

/// <summary>Default slide layout — title and content arranged vertically.</summary>
public sealed class DefaultSlideLayout : SlideLayoutBase
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        await ComponentRenderer.RenderSliceAsync<DefaultSlideLayoutTemplate, SlideLayoutModel>(
            context.Destination, BuildModel(), BuildSlots(context));
    }
}

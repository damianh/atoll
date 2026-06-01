using Atoll.Components;

namespace Atoll.Swell.Layouts;

/// <summary>End/closing slide layout — centered content, suitable for "Thank you" or Q&amp;A slides.</summary>
public sealed class EndSlideLayout : SlideLayoutBase
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        await ComponentRenderer.RenderSliceAsync<EndSlideLayoutTemplate, SlideLayoutModel>(
            context.Destination, BuildModel(), BuildSlots(context));
    }
}

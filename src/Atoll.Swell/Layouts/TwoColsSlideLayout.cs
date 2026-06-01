using Atoll.Components;

namespace Atoll.Swell.Layouts;

/// <summary>
/// Two-column slide layout. Left column receives content before <c>::right::</c>;
/// right column receives content after <c>::right::</c>.
/// </summary>
public sealed class TwoColsSlideLayout : SlideLayoutBase
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        await ComponentRenderer.RenderSliceAsync<TwoColsSlideLayoutTemplate, SlideLayoutModel>(
            context.Destination, BuildModel(), BuildSlots(context));
    }
}

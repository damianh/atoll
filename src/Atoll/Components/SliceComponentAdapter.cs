using System.Text.Encodings.Web;
using System.Text.Unicode;
using Atoll.Rendering;
using Atoll.Slots;
using RazorSlices;

namespace Atoll.Components;

/// <summary>
/// Adapts a <see cref="RazorSlice"/> (or subclass) to the <see cref="IAtollComponent"/> interface,
/// enabling Razor-authored slices to participate in the Atoll component rendering pipeline.
/// </summary>
/// <remarks>
/// <para>
/// When <see cref="RenderAsync"/> is called, this adapter:
/// <list type="number">
/// <item><description>Extracts the <see cref="IRenderDestination"/> from the context.</description></item>
/// <item><description>If the slice is an <see cref="AtollSlice"/> or <see cref="AtollSlice{TModel}"/>,
/// injects <c>Destination</c> and <c>Slots</c> so Razor template helpers work correctly.</description></item>
/// <item><description>Creates a <see cref="RenderDestinationTextWriter"/> wrapping the same destination.</description></item>
/// <item><description>Calls the Razor slice's <c>RenderAsync</c> method to execute the Razor template.</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class SliceComponentAdapter : IAtollComponent
{
    private readonly RazorSlice _slice;

    /// <summary>
    /// Initializes a new <see cref="SliceComponentAdapter"/> wrapping the specified slice.
    /// </summary>
    /// <param name="slice">The Razor slice to adapt.</param>
    public SliceComponentAdapter(RazorSlice slice)
    {
        ArgumentNullException.ThrowIfNull(slice);
        _slice = slice;
    }

    /// <summary>
    /// HTML encoder that passes all Unicode characters through, matching Atoll's built-in encoding behavior.
    /// </summary>
    private static readonly System.Text.Encodings.Web.HtmlEncoder PermissiveHtmlEncoder =
        System.Text.Encodings.Web.HtmlEncoder.Create(UnicodeRanges.All);

    /// <inheritdoc />
    public async Task RenderAsync(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var destination = context.Destination;
        var slots = context.Slots;

        InjectAtollContext(destination, slots);

        await using var writer = new RenderDestinationTextWriter(destination);
        await _slice.RenderAsync(writer, PermissiveHtmlEncoder);
    }

    /// <summary>
    /// Injects <see cref="IRenderDestination"/> and <see cref="SlotCollection"/> into the slice
    /// if it derives from <see cref="AtollSlice"/> or <see cref="AtollSlice{TModel}"/>.
    /// </summary>
    private void InjectAtollContext(IRenderDestination destination, SlotCollection slots)
    {
        if (_slice is IAtollSliceContext atollContext)
        {
            atollContext.Destination = destination;
            atollContext.Slots = slots;
        }
    }
}

/// <summary>
/// Internal interface for injecting Atoll context into AtollSlice and AtollSlice&lt;TModel&gt;.
/// Avoids the need for reflection to set internal properties.
/// </summary>
internal interface IAtollSliceContext
{
    IRenderDestination? Destination { get; set; }
    SlotCollection Slots { get; set; }
}

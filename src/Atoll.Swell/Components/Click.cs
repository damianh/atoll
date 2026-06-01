using Atoll.Components;

namespace Atoll.Swell.Components;

/// <summary>
/// A progressive-reveal wrapper component. Use the <c>:::Click</c> block directive in a
/// Swell Markdown slide to wrap content that should be revealed on successive key presses
/// or clicks within the current slide.
/// </summary>
/// <remarks>
/// <para>
/// Server-side rendering wraps the slot content in
/// <c>&lt;div class="swell-click"&gt;…&lt;/div&gt;</c>.
/// The navigation island (<c>swell-nav.js</c>) tracks how many <c>.swell-click</c> blocks
/// have been revealed on the current slide and progressively applies
/// <c>swell-click-visible</c> to each in document order on forward navigation.
/// On backward navigation, the last revealed block is hidden again.
/// </para>
/// <para>
/// Example usage in a slide:
/// <code>
/// :::Click
/// - Revealed on first click
/// :::
///
/// :::Click
/// - Revealed on second click
/// :::
/// </code>
/// </para>
/// </remarks>
public sealed class Click : AtollComponent
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"swell-click\">");
        await RenderSlotAsync();
        WriteHtml("</div>");
    }
}

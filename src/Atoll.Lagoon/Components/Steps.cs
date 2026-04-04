using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A styled ordered-list wrapper for step-by-step instructions.
/// CSS counter-based numbering handles the visual circles and connecting lines.
/// Slot content is expected to contain an <c>&lt;ol&gt;</c> element.
/// </summary>
public sealed class Steps : AtollComponent
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"steps\">");
        await RenderSlotAsync();
        WriteHtml("</div>");
    }
}

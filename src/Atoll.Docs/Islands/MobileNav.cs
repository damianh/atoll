using Atoll.Components;
using Atoll.Islands;

namespace Atoll.Docs.Islands;

/// <summary>
/// A mobile navigation island that renders a hamburger menu button for small screens.
/// Only hydrates when the viewport matches <c>(max-width: 768px)</c>, so the JS is
/// not loaded on desktop at all.
/// </summary>
/// <remarks>
/// The sidebar overlay with <c>id="mobile-nav-menu"</c> must exist in the page layout.
/// The client script shows/hides it, traps focus when open, and closes on Escape.
/// </remarks>
[ClientMedia("(max-width: 768px)")]
public sealed class MobileNav : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-docs-mobile-nav.js";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <button id="mobile-nav-toggle" type="button" aria-label="Open navigation" aria-expanded="false" aria-controls="mobile-nav-menu">&#9776;</button>
            """);
        return Task.CompletedTask;
    }
}

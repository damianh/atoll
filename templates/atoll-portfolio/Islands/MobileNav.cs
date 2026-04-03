using Atoll.Components;
using Atoll.Islands;

namespace AtollPortfolio.Islands;

/// <summary>
/// A mobile navigation menu island that only hydrates on small screens.
/// Uses <c>client:media("(max-width: 768px)")</c> so the hamburger menu
/// JavaScript is only loaded on mobile viewports.
/// Demonstrates the <see cref="ClientMediaAttribute"/> directive.
/// </summary>
[ClientMedia("(max-width: 768px)")]
public sealed class MobileNav : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/mobile-nav.js";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <div class="mobile-nav">
                <button id="mobile-nav-toggle" type="button" aria-label="Toggle navigation"
                    style="display: none; background: none; border: 1px solid var(--color-border); border-radius: 0.25rem; padding: 0.375rem 0.625rem; cursor: pointer; color: var(--color-text); font-size: 1.25rem;">
                    &#9776;
                </button>
                <div id="mobile-nav-menu" style="display: none; position: fixed; inset: 0; background: var(--color-bg); z-index: 200; padding: 2rem;">
                    <button id="mobile-nav-close" type="button" aria-label="Close navigation"
                        style="position: absolute; top: 1rem; right: 1rem; background: none; border: none; color: var(--color-text); font-size: 1.5rem; cursor: pointer;">
                        &times;
                    </button>
                    <nav style="display: flex; flex-direction: column; gap: 1.5rem; padding-top: 3rem; text-align: center;">
                        <a href="/" style="font-size: 1.25rem; color: var(--color-heading);">Home</a>
                        <a href="/projects" style="font-size: 1.25rem; color: var(--color-heading);">Projects</a>
                        <a href="/about" style="font-size: 1.25rem; color: var(--color-heading);">About</a>
                        <a href="/contact" style="font-size: 1.25rem; color: var(--color-heading);">Contact</a>
                    </nav>
                </div>
            </div>
            """);
        return Task.CompletedTask;
    }
}

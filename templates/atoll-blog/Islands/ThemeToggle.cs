using Atoll.Components;
using Atoll.Islands;

namespace AtollBlog.Islands;

/// <summary>
/// A theme toggle island component that renders a button for switching
/// between light and dark themes. Uses <c>client:load</c> for immediate hydration.
/// </summary>
[ClientLoad]
public sealed class ThemeToggle : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/theme-toggle.js";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <button id="theme-toggle" type="button" aria-label="Toggle theme"
                style="background: none; border: 1px solid var(--color-border); border-radius: 0.25rem; padding: 0.25rem 0.5rem; cursor: pointer; font-size: 1rem;"
                >&#9788;</button>
            """);
        return Task.CompletedTask;
    }
}

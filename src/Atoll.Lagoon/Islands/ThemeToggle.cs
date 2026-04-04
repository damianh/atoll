using Atoll.Components;
using Atoll.Islands;

namespace Atoll.Lagoon.Islands;

/// <summary>
/// A dark/light theme toggle island that renders a button for switching between
/// light and dark themes. Uses <c>client:load</c> for immediate hydration so the
/// correct theme icon is shown as early as possible.
/// </summary>
/// <remarks>
/// The client script reads <c>localStorage</c> for a persisted preference,
/// falls back to <c>prefers-color-scheme</c>, and sets <c>data-theme</c> on
/// the <c>&lt;html&gt;</c> element. Toggling persists the new choice.
/// </remarks>
[ClientLoad]
public sealed class ThemeToggle : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-docs-theme-toggle.js";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <button id="theme-toggle" type="button" aria-label="Toggle theme">☾</button>
            """);
        return Task.CompletedTask;
    }
}

using Atoll.Core.Components;

namespace Atoll.Samples.Portfolio.Components;

/// <summary>
/// Renders a skill badge as a styled inline element with a name and proficiency level.
/// </summary>
public sealed class SkillBadge : AtollComponent
{
    /// <summary>
    /// Gets or sets the skill name.
    /// </summary>
    [Parameter(Required = true)]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the proficiency level (e.g., "Expert", "Advanced", "Intermediate").
    /// </summary>
    [Parameter]
    public string Level { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        var levelColor = Level.ToLowerInvariant() switch
        {
            "expert" => "var(--color-success)",
            "advanced" => "var(--color-primary)",
            _ => "var(--color-accent)",
        };

        WriteHtml("<div style=\"display: inline-flex; align-items: center; gap: 0.5rem; background: var(--color-surface); border: 1px solid var(--color-border); padding: 0.375rem 0.75rem; border-radius: 0.375rem;\">");
        WriteHtml("<span style=\"font-size: 0.875rem; color: var(--color-heading);\">");
        WriteText(Name);
        WriteHtml("</span>");

        if (!string.IsNullOrEmpty(Level))
        {
            WriteHtml("<span style=\"font-size: 0.75rem; color: ");
            WriteHtml(levelColor);
            WriteHtml("; font-family: var(--font-mono);\">");
            WriteText(Level);
            WriteHtml("</span>");
        }

        WriteHtml("</div>");
        return Task.CompletedTask;
    }
}

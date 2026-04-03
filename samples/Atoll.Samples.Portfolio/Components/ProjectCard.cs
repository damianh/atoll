using Atoll.Components;

namespace Atoll.Samples.Portfolio.Components;

/// <summary>
/// Renders a project card with title, description, technology tags,
/// and links to the live demo and source code.
/// </summary>
public sealed class ProjectCard : AtollComponent
{
    /// <summary>
    /// Gets or sets the project title.
    /// </summary>
    [Parameter(Required = true)]
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    [Parameter(Required = true)]
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the comma-separated list of technologies used.
    /// </summary>
    [Parameter]
    public string Technologies { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL to the live demo.
    /// </summary>
    [Parameter]
    public string DemoUrl { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL to the source code repository.
    /// </summary>
    [Parameter]
    public string SourceUrl { get; set; } = "";

    /// <summary>
    /// Gets or sets the project image URL for the card thumbnail.
    /// </summary>
    [Parameter]
    public string ImageUrl { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <article style="background: var(--color-surface); border: 1px solid var(--color-border); border-radius: 0.75rem; overflow: hidden; transition: transform 0.2s, box-shadow 0.2s;">
            """);

        if (!string.IsNullOrEmpty(ImageUrl))
        {
            WriteHtml("<div style=\"height: 12rem; background: var(--color-border); display: flex; align-items: center; justify-content: center; overflow: hidden;\">");
            WriteHtml("<img src=\"");
            WriteText(ImageUrl);
            WriteHtml("\" alt=\"");
            WriteText(Title);
            WriteHtml("\" style=\"width: 100%; height: 100%; object-fit: cover;\" />");
            WriteHtml("</div>");
        }
        else
        {
            WriteHtml("""
                <div style="height: 12rem; background: linear-gradient(135deg, var(--color-primary), var(--color-accent)); display: flex; align-items: center; justify-content: center;">
                    <span style="font-size: 3rem; opacity: 0.3;">&#9998;</span>
                </div>
                """);
        }

        WriteHtml("<div style=\"padding: 1.5rem;\">");
        WriteHtml("<h3 style=\"font-size: 1.25rem; color: var(--color-heading); margin-bottom: 0.5rem;\">");
        WriteText(Title);
        WriteHtml("</h3>");
        WriteHtml("<p style=\"color: var(--color-muted); font-size: 0.9375rem; margin-bottom: 1rem; line-height: 1.6;\">");
        WriteText(Description);
        WriteHtml("</p>");

        if (!string.IsNullOrEmpty(Technologies))
        {
            WriteHtml("<div style=\"display: flex; gap: 0.5rem; flex-wrap: wrap; margin-bottom: 1rem;\">");
            var techs = Technologies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var tech in techs)
            {
                WriteHtml("<span style=\"background: var(--color-bg); color: var(--color-primary); padding: 0.125rem 0.5rem; border-radius: 0.25rem; font-size: 0.75rem; font-family: var(--font-mono);\">");
                WriteText(tech);
                WriteHtml("</span>");
            }

            WriteHtml("</div>");
        }

        var hasDemoUrl = !string.IsNullOrEmpty(DemoUrl);
        var hasSourceUrl = !string.IsNullOrEmpty(SourceUrl);
        if (hasDemoUrl || hasSourceUrl)
        {
            WriteHtml("<div style=\"display: flex; gap: 1rem;\">");
            if (hasDemoUrl)
            {
                WriteHtml("<a href=\"");
                WriteText(DemoUrl);
                WriteHtml("\" style=\"color: var(--color-primary); font-size: 0.875rem; font-weight: 600;\">Live Demo &rarr;</a>");
            }

            if (hasSourceUrl)
            {
                WriteHtml("<a href=\"");
                WriteText(SourceUrl);
                WriteHtml("\" style=\"color: var(--color-muted); font-size: 0.875rem;\">Source Code</a>");
            }

            WriteHtml("</div>");
        }

        WriteHtml("</div>");
        WriteHtml("</article>");
        return Task.CompletedTask;
    }
}

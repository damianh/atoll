using Atoll.Components;

namespace AtollPortfolio.Components;

/// <summary>
/// Renders the hero section on the portfolio home page with name,
/// title, brief intro, and call-to-action buttons.
/// </summary>
public sealed class HeroSection : AtollComponent
{
    /// <summary>
    /// Gets or sets the person's name displayed prominently.
    /// </summary>
    [Parameter(Required = true)]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the professional title or tagline.
    /// </summary>
    [Parameter(Required = true)]
    public string Tagline { get; set; } = "";

    /// <summary>
    /// Gets or sets the short introduction paragraph.
    /// </summary>
    [Parameter]
    public string Intro { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <section style="text-align: center; padding: 5rem 1.5rem; max-width: 48rem; margin: 0 auto;">
                <p style="color: var(--color-primary); font-family: var(--font-mono); font-size: 1rem; margin-bottom: 1rem;">
                    Hello, I'm
                </p>
                <h1 style="font-size: 3rem; font-weight: 800; color: var(--color-heading); margin-bottom: 0.5rem;">
            """);
        WriteText(Name);
        WriteHtml("""
                </h1>
                <p style="font-size: 1.5rem; color: var(--color-accent); margin-bottom: 1.5rem;">
            """);
        WriteText(Tagline);
        WriteHtml("</p>");

        if (!string.IsNullOrEmpty(Intro))
        {
            WriteHtml("<p style=\"font-size: 1.125rem; color: var(--color-muted); max-width: 36rem; margin: 0 auto 2rem;\">");
            WriteText(Intro);
            WriteHtml("</p>");
        }

        WriteHtml("""
                <div style="display: flex; gap: 1rem; justify-content: center; flex-wrap: wrap;">
                    <a href="/projects" class="btn-primary">View My Work</a>
                    <a href="/contact" style="display: inline-block; border: 1px solid var(--color-primary); color: var(--color-primary); padding: 0.75rem 1.5rem; border-radius: 0.5rem; font-weight: 600; transition: background 0.2s;">
                        Get in Touch
                    </a>
                </div>
            </section>
            """);
        return Task.CompletedTask;
    }
}

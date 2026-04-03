using Atoll.Components;
using Atoll.Routing;
using AtollPortfolio.Layouts;

namespace AtollPortfolio.Pages;

/// <summary>
/// The about page with biography and skills.
/// </summary>
[Layout(typeof(PortfolioLayout))]
public sealed class AboutPage : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <section class="container" style="padding: 3rem 1.5rem; max-width: 48rem; margin: 0 auto;">
                <h1 style="font-size: 2.25rem; color: var(--color-heading); margin-bottom: 1.5rem;">About Me</h1>
                <div style="color: var(--color-text); line-height: 1.8;">
                    <p style="margin-bottom: 1rem;">
                        I'm a full-stack developer with experience building web applications
                        and distributed systems with .NET. I'm passionate about creating performant,
                        maintainable software.
                    </p>
                    <p style="margin-bottom: 2rem;">
                        Customize this page to tell your own story.
                    </p>
                </div>
                <h2 style="font-size: 1.5rem; color: var(--color-heading); margin-bottom: 1rem;">Skills &amp; Technologies</h2>
                <div style="display: flex; flex-wrap: wrap; gap: 0.5rem; margin-bottom: 3rem;">
            """);

        var skills = new[] { "C#", "ASP.NET Core", ".NET", "SQL Server", "Docker", "Azure" };
        foreach (var skill in skills)
        {
            WriteHtml("<span style=\"background: var(--color-surface); color: var(--color-primary); border: 1px solid var(--color-border); padding: 0.25rem 0.75rem; border-radius: 9999px; font-size: 0.875rem; font-family: var(--font-mono);\">");
            WriteText(skill);
            WriteHtml("</span>");
        }

        WriteHtml("""
                </div>
            </section>
            """);
        return Task.CompletedTask;
    }
}

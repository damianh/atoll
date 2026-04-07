using Atoll.Components;
using Atoll.Routing;
using Atoll.Samples.Portfolio.Components;
using Atoll.Samples.Portfolio.Islands;
using Atoll.Samples.Portfolio.Layouts;

namespace Atoll.Samples.Portfolio.Pages;

/// <summary>
/// The about page with biography, skills, and an image gallery island.
/// Demonstrates the <see cref="ImageGallery"/> island with <c>client:visible</c>.
/// </summary>
[Layout(typeof(PortfolioLayout))]
public sealed class AboutPage : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <section class="container" style="padding: 3rem 1.5rem; max-width: 48rem; margin: 0 auto;">
                <h1 style="font-size: 2.25rem; color: var(--color-heading); margin-bottom: 1.5rem;">About Me</h1>
                <div style="color: var(--color-text); line-height: 1.8;">
                    <p style="margin-bottom: 1rem;">
                        I'm a full-stack developer with over 8 years of experience building web applications
                        and distributed systems with .NET. I'm passionate about creating performant, maintainable
                        software and contributing to open-source projects.
                    </p>
                    <p style="margin-bottom: 1rem;">
                        My journey started with desktop applications in WinForms and WPF, evolved through
                        ASP.NET MVC and Web API, and now focuses on modern architectures using ASP.NET Core,
                        Blazor, and frameworks like Atoll that push the boundaries of what .NET can do on the web.
                    </p>
                    <p style="margin-bottom: 2rem;">
                        When I'm not coding, you'll find me hiking, photographing landscapes, or experimenting
                        with home automation and IoT projects.
                    </p>
                </div>
            """);

        // Skills section
        WriteHtml("""
                <h2 style="font-size: 1.5rem; color: var(--color-heading); margin-bottom: 1rem;">Skills &amp; Technologies</h2>
                <div style="display: flex; flex-wrap: wrap; gap: 0.5rem; margin-bottom: 3rem;">
            """);

        var skills = GetSkills();
        foreach (var skillProps in skills)
        {
            var skillFragment = ComponentRenderer.ToFragment<SkillBadge>(skillProps);
            await RenderAsync(skillFragment);
        }

        WriteHtml("</div>");

        // Image gallery section (client:visible island)
        WriteHtml("""
                <h2 style="font-size: 1.5rem; color: var(--color-heading); margin-bottom: 1rem;">Photo Gallery</h2>
                <p style="color: var(--color-muted); margin-bottom: 1.5rem;">Some of my favorite shots from recent adventures.</p>
            """);

        var galleryProps = new Dictionary<string, object?>
        {
            ["ImageUrls"] = "",
            ["Captions"] = "",
        };
        var galleryFragment = ComponentRenderer.ToFragment<ImageGallery>(galleryProps);
        await RenderAsync(galleryFragment);

        WriteHtml("</section>");
    }

    private static List<Dictionary<string, object?>> GetSkills()
    {
        return
        [
            new Dictionary<string, object?> { ["Name"] = "C#", ["Level"] = "Expert" },
            new Dictionary<string, object?> { ["Name"] = "ASP.NET Core", ["Level"] = "Expert" },
            new Dictionary<string, object?> { ["Name"] = ".NET", ["Level"] = "Expert" },
            new Dictionary<string, object?> { ["Name"] = "Blazor", ["Level"] = "Advanced" },
            new Dictionary<string, object?> { ["Name"] = "SQL Server", ["Level"] = "Advanced" },
            new Dictionary<string, object?> { ["Name"] = "PostgreSQL", ["Level"] = "Advanced" },
            new Dictionary<string, object?> { ["Name"] = "Docker", ["Level"] = "Advanced" },
            new Dictionary<string, object?> { ["Name"] = "Azure", ["Level"] = "Advanced" },
            new Dictionary<string, object?> { ["Name"] = "TypeScript", ["Level"] = "Intermediate" },
            new Dictionary<string, object?> { ["Name"] = "React", ["Level"] = "Intermediate" },
            new Dictionary<string, object?> { ["Name"] = "Redis", ["Level"] = "Intermediate" },
            new Dictionary<string, object?> { ["Name"] = "GraphQL", ["Level"] = "Intermediate" },
        ];
    }
}

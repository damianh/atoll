using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using Atoll.Samples.Portfolio.Components;
using Atoll.Samples.Portfolio.Layouts;

namespace Atoll.Samples.Portfolio.Pages;

/// <summary>
/// The portfolio home page. Displays a hero section with personal introduction
/// and a featured projects preview.
/// </summary>
[Layout(typeof(PortfolioLayout))]
public sealed class IndexPage : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        // Hero section
        var heroProps = new Dictionary<string, object?>
        {
            ["Name"] = "Alex Chen",
            ["Tagline"] = "Full-Stack .NET Developer",
            ["Intro"] = "I build modern web applications with C#, ASP.NET Core, and the Atoll framework. Passionate about clean code, performance, and developer experience.",
        };
        var heroFragment = ComponentRenderer.ToFragment<HeroSection>(heroProps);
        await RenderAsync(heroFragment);

        // Featured projects section
        WriteHtml("""
            <section style="padding: 4rem 1.5rem; max-width: 72rem; margin: 0 auto;">
                <h2 style="font-size: 1.75rem; color: var(--color-heading); text-align: center; margin-bottom: 0.5rem;">Featured Projects</h2>
                <p style="text-align: center; color: var(--color-muted); margin-bottom: 2.5rem;">A selection of my recent work</p>
                <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(20rem, 1fr)); gap: 1.5rem;">
            """);

        var featuredProjects = GetFeaturedProjects();
        foreach (var projectProps in featuredProjects)
        {
            var cardFragment = ComponentRenderer.ToFragment<ProjectCard>(projectProps);
            await RenderAsync(cardFragment);
        }

        WriteHtml("""
                </div>
                <div style="text-align: center; margin-top: 2.5rem;">
                    <a href="/projects" class="btn-primary">View All Projects &rarr;</a>
                </div>
            </section>
            """);
    }

    private static List<Dictionary<string, object?>> GetFeaturedProjects()
    {
        return
        [
            new Dictionary<string, object?>
            {
                ["Title"] = "Atoll Framework",
                ["Description"] = "A .NET-native web framework inspired by Astro with islands architecture and zero JS by default.",
                ["Technologies"] = "C#, ASP.NET Core, Blazor WASM",
                ["DemoUrl"] = "https://atoll.dev",
                ["SourceUrl"] = "https://github.com/example/atoll",
            },
            new Dictionary<string, object?>
            {
                ["Title"] = "Cloud Dashboard",
                ["Description"] = "Real-time monitoring dashboard for cloud infrastructure with interactive charts and alerting.",
                ["Technologies"] = "C#, SignalR, React, Azure",
                ["DemoUrl"] = "https://dashboard.example.com",
                ["SourceUrl"] = "",
            },
        ];
    }
}

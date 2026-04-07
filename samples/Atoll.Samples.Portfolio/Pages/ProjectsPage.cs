using Atoll.Components;
using Atoll.Routing;
using Atoll.Samples.Portfolio.Components;
using Atoll.Samples.Portfolio.Layouts;

namespace Atoll.Samples.Portfolio.Pages;

/// <summary>
/// The projects listing page. Displays all projects as a responsive grid of cards.
/// </summary>
[Layout(typeof(PortfolioLayout))]
public sealed class ProjectsPage : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <section class="container" style="padding: 3rem 1.5rem;">
                <h1 style="font-size: 2.25rem; color: var(--color-heading); margin-bottom: 0.5rem;">Projects</h1>
                <p style="color: var(--color-muted); margin-bottom: 2.5rem; max-width: 36rem;">
                    Here are some of the projects I've worked on. Each showcases different
                    technologies and problem-solving approaches.
                </p>
                <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(20rem, 1fr)); gap: 1.5rem;">
            """);

        var projects = GetAllProjects();
        foreach (var projectProps in projects)
        {
            var cardFragment = ComponentRenderer.ToFragment<ProjectCard>(projectProps);
            await RenderAsync(cardFragment);
        }

        WriteHtml("""
                </div>
            </section>
            """);
    }

    private static List<Dictionary<string, object?>> GetAllProjects()
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
            new Dictionary<string, object?>
            {
                ["Title"] = "E-Commerce API",
                ["Description"] = "High-performance REST API for an e-commerce platform handling 10k requests per second.",
                ["Technologies"] = "C#, PostgreSQL, Redis, Docker",
                ["DemoUrl"] = "",
                ["SourceUrl"] = "https://github.com/example/ecommerce-api",
            },
            new Dictionary<string, object?>
            {
                ["Title"] = "DevOps Toolkit",
                ["Description"] = "CLI tool suite for automating development workflows, CI/CD pipelines, and infrastructure provisioning.",
                ["Technologies"] = "C#, System.CommandLine, YAML",
                ["DemoUrl"] = "",
                ["SourceUrl"] = "https://github.com/example/devops-toolkit",
            },
            new Dictionary<string, object?>
            {
                ["Title"] = "Weather Station",
                ["Description"] = "IoT weather station with real-time sensor data collection, storage, and visualization.",
                ["Technologies"] = "C#, MQTT, InfluxDB, Grafana",
                ["DemoUrl"] = "https://weather.example.com",
                ["SourceUrl"] = "https://github.com/example/weather-station",
            },
            new Dictionary<string, object?>
            {
                ["Title"] = "Markdown Editor",
                ["Description"] = "Browser-based Markdown editor with live preview, syntax highlighting, and export to PDF.",
                ["Technologies"] = "TypeScript, Web Components, Markdig",
                ["DemoUrl"] = "https://editor.example.com",
                ["SourceUrl"] = "https://github.com/example/md-editor",
            },
        ];
    }
}

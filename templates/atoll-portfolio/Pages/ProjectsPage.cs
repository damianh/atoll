using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using AtollPortfolio.Components;
using AtollPortfolio.Layouts;

namespace AtollPortfolio.Pages;

/// <summary>
/// The projects listing page. Displays all projects as a responsive grid of cards.
/// </summary>
[Layout(typeof(PortfolioLayout))]
[PageRoute("/projects")]
public sealed class ProjectsPage : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <section class="container" style="padding: 3rem 1.5rem;">
                <h1 style="font-size: 2.25rem; color: var(--color-heading); margin-bottom: 0.5rem;">Projects</h1>
                <p style="color: var(--color-muted); margin-bottom: 2.5rem; max-width: 36rem;">
                    Here are some of the projects I've worked on.
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
                ["Title"] = "My First Project",
                ["Description"] = "A description of your first project.",
                ["Technologies"] = "C#, ASP.NET Core",
                ["DemoUrl"] = "",
                ["SourceUrl"] = "https://github.com/yourusername/project-one",
            },
            new Dictionary<string, object?>
            {
                ["Title"] = "My Second Project",
                ["Description"] = "A description of your second project.",
                ["Technologies"] = "C#, PostgreSQL, Docker",
                ["DemoUrl"] = "https://example.com",
                ["SourceUrl"] = "",
            },
            new Dictionary<string, object?>
            {
                ["Title"] = "My Third Project",
                ["Description"] = "A description of your third project.",
                ["Technologies"] = "TypeScript, React",
                ["DemoUrl"] = "",
                ["SourceUrl"] = "https://github.com/yourusername/project-three",
            },
        ];
    }
}

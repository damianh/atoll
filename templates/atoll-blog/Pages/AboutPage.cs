using Atoll.Components;
using Atoll.Routing;
using AtollBlog.Layouts;

namespace AtollBlog.Pages;

/// <summary>
/// The about page.
/// </summary>
[Layout(typeof(BlogLayout))]
[PageRoute("/about")]
public sealed class AboutPage : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <h1 style="margin-bottom: 1rem;">About This Blog</h1>
            <p>
                This blog is built with <strong>Atoll</strong>, a .NET-native framework
                inspired by <a href="https://astro.build">Astro</a>.
            </p>
            <h2 style="margin-top: 2rem; margin-bottom: 0.5rem;">About Atoll</h2>
            <ul style="margin: 1rem 0; padding-left: 1.5rem;">
                <li><strong>Server-first rendering</strong> &mdash; Zero JavaScript by default</li>
                <li><strong>Islands architecture</strong> &mdash; Hydrate only what needs to be interactive</li>
                <li><strong>Content collections</strong> &mdash; Type-safe Markdown with frontmatter validation</li>
                <li><strong>C# components</strong> &mdash; Pure C# with raw string literals</li>
            </ul>
            """);
        return Task.CompletedTask;
    }
}

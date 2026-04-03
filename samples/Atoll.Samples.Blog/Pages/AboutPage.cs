using Atoll.Components;
using Atoll.Routing;
using Atoll.Samples.Blog.Layouts;

namespace Atoll.Samples.Blog.Pages;

/// <summary>
/// The about page with site and author information.
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
                This is a sample blog site built with <strong>Atoll</strong>, a .NET-native
                framework inspired by <a href="https://astro.build">Astro</a>.
            </p>
            <h2 style="margin-top: 2rem; margin-bottom: 0.5rem;">About Atoll</h2>
            <p>Atoll brings Astro's core concepts to the .NET ecosystem:</p>
            <ul style="margin: 1rem 0; padding-left: 1.5rem;">
                <li><strong>Server-first rendering</strong> &mdash; Zero JavaScript by default</li>
                <li><strong>Islands architecture</strong> &mdash; Hydrate only what needs to be interactive</li>
                <li><strong>Content collections</strong> &mdash; Type-safe Markdown with frontmatter validation</li>
                <li><strong>File-based routing</strong> &mdash; Convention over configuration</li>
                <li><strong>C# components</strong> &mdash; No template files, pure C# with raw string literals</li>
            </ul>
            <h2 style="margin-top: 2rem; margin-bottom: 0.5rem;">The Author</h2>
            <p>
                Built by the Atoll contributors. See the
                <a href="https://github.com/example/atoll">source code on GitHub</a>.
            </p>
            """);
        return Task.CompletedTask;
    }
}

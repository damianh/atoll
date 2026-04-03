using Atoll.Components;
using Atoll.Routing;
using Atoll.Samples.Blog.Layouts;

namespace Atoll.Samples.Blog.Pages;

/// <summary>
/// The blog home page. Displays a welcome message and links to recent posts.
/// </summary>
[Layout(typeof(BlogLayout))]
[PageRoute("/")]
public sealed class IndexPage : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <section style="text-align: center; padding: 3rem 0;">
                <h1 style="font-size: 2.5rem; margin-bottom: 1rem;">Welcome to Atoll Blog</h1>
                <p style="font-size: 1.125rem; color: var(--color-muted); max-width: 32rem; margin: 0 auto;">
                    A sample blog built with Atoll &mdash; demonstrating layouts, components,
                    content collections, islands, and more.
                </p>
                <div style="margin-top: 2rem;">
                    <a href="/blog" style="display: inline-block; background: var(--color-primary); color: white; padding: 0.75rem 1.5rem; border-radius: 0.375rem; font-weight: 600;">
                        Read the Blog &rarr;
                    </a>
                </div>
            </section>
            """);
        return Task.CompletedTask;
    }
}

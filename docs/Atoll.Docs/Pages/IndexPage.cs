using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Routing;
using Atoll.Docs.Layouts;

namespace Atoll.Docs.Pages;

/// <summary>
/// The documentation site landing page. Displays a hero section with a tagline
/// and feature highlights, linking visitors to the Getting Started guide.
/// </summary>
[Layout(typeof(DocsLayout))]
[PageRoute("/")]
public sealed class IndexPage : AtollComponent, IAtollPage
{
    /// <summary>
    /// Gets or sets the collection query used by the layout's sidebar navigation.
    /// </summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <div class="hero">
                <h1>Atoll</h1>
                <p class="tagline">A .NET-native static-site framework inspired by Astro. Build fast, content-rich sites with pure C# — no JavaScript required.</p>
                <a href="/docs/getting-started" class="cta">Get Started &rarr;</a>
                <div class="features">
                    <div class="feature-card">
                        <h3>Server-First Rendering</h3>
                        <p>All components render to static HTML on the server. Zero JavaScript overhead by default.</p>
                    </div>
                    <div class="feature-card">
                        <h3>Content Collections</h3>
                        <p>Type-safe Markdown content with YAML frontmatter validation using your own schema classes.</p>
                    </div>
                    <div class="feature-card">
                        <h3>Layouts &amp; Components</h3>
                        <p>Compose pages from reusable C# components with a familiar attribute-based routing model.</p>
                    </div>
                    <div class="feature-card">
                        <h3>Islands Architecture</h3>
                        <p>Opt-in client-side interactivity with <code>client:load</code> directives — hydrate only what you need.</p>
                    </div>
                    <div class="feature-card">
                        <h3>Static Site Generation</h3>
                        <p>Generate a fully static site with <code>StaticSiteGenerator</code> — deploy anywhere with no server required.</p>
                    </div>
                    <div class="feature-card">
                        <h3>CSS Scoping</h3>
                        <p>Automatic CSS class scoping keeps your component styles isolated with no naming conflicts.</p>
                    </div>
                </div>
            </div>
            """);

        return Task.CompletedTask;
    }
}

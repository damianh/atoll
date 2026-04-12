using Atoll.Components;

namespace Atoll.Samples.Blog.Layouts;

/// <summary>
/// The main layout for the blog site. Provides the HTML structure with
/// header navigation, main content area, and footer.
/// </summary>
public sealed class BlogLayout : AtollComponent
{
    /// <summary>
    /// Gets or sets the page title. Used in the &lt;title&gt; element.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Atoll Blog";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>
            """);
        WriteText(Title);
        WriteHtml("""
                </title>
                <style>
                    :root { --color-bg: #fafafa; --color-text: #1a1a2e; --color-primary: #0f3460; --color-accent: #e94560; --color-muted: #666; --color-border: #e0e0e0; }
                    *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
                    body { font-family: system-ui, -apple-system, sans-serif; background: var(--color-bg); color: var(--color-text); line-height: 1.7; }
                    a { color: var(--color-primary); text-decoration: none; }
                    a:hover { color: var(--color-accent); }
                    .container { max-width: 48rem; margin: 0 auto; padding: 0 1.5rem; }
                </style>
            </head>
            <body>
                <header style="border-bottom: 1px solid var(--color-border); padding: 1rem 0;">
                    <nav class="container" style="display: flex; justify-content: space-between; align-items: center;">
                        <a href="/" style="font-size: 1.25rem; font-weight: 700;">Atoll Blog</a>
                        <div style="display: flex; gap: 1.5rem;">
                            <a href="/">Home</a>
                            <a href="/blog">Posts</a>
                            <a href="/tags">Tags</a>
                            <a href="/about">About</a>
                        </div>
                    </nav>
                </header>
                <main class="container" style="padding: 2rem 1.5rem; min-height: 60vh;">
            """);
        await RenderSlotAsync();
        WriteHtml("""
                </main>
                <footer style="border-top: 1px solid var(--color-border); padding: 1.5rem 0; text-align: center; color: var(--color-muted); font-size: 0.875rem;">
                    <div class="container">
                        <p>Built with Atoll &mdash; the static site generator for .NET.</p>
                    </div>
                </footer>
            </body>
            </html>
            """);
    }
}

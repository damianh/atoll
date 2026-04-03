using Atoll.Components;

namespace AtollPortfolio.Layouts;

/// <summary>
/// The main layout for the portfolio site. Provides the HTML structure with
/// a responsive navigation header, main content area, and footer.
/// </summary>
public sealed class PortfolioLayout : AtollComponent
{
    /// <summary>
    /// Gets or sets the page title. Used in the &lt;title&gt; element.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "AtollPortfolio";

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
                    :root {
                        --color-bg: #0f172a;
                        --color-surface: #1e293b;
                        --color-text: #e2e8f0;
                        --color-heading: #f8fafc;
                        --color-primary: #38bdf8;
                        --color-accent: #a78bfa;
                        --color-muted: #94a3b8;
                        --color-border: #334155;
                        --font-sans: system-ui, -apple-system, sans-serif;
                        --font-mono: 'Fira Code', 'Cascadia Code', monospace;
                        --max-width: 72rem;
                    }
                    *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
                    body { font-family: var(--font-sans); background: var(--color-bg); color: var(--color-text); line-height: 1.7; }
                    a { color: var(--color-primary); text-decoration: none; transition: color 0.2s; }
                    a:hover { color: var(--color-accent); }
                    .container { max-width: var(--max-width); margin: 0 auto; padding: 0 1.5rem; }
                    .btn-primary {
                        display: inline-block; background: var(--color-primary); color: var(--color-bg);
                        padding: 0.75rem 1.5rem; border-radius: 0.5rem; font-weight: 600;
                        transition: background 0.2s, transform 0.1s;
                    }
                    .btn-primary:hover { background: var(--color-accent); color: var(--color-bg); transform: translateY(-1px); }
                </style>
            </head>
            <body>
                <header style="border-bottom: 1px solid var(--color-border); padding: 1rem 0; position: sticky; top: 0; background: var(--color-bg); z-index: 100;">
                    <nav class="container" style="display: flex; justify-content: space-between; align-items: center;">
                        <a href="/" style="font-size: 1.25rem; font-weight: 700; color: var(--color-heading);">
                            &lt;dev /&gt;
                        </a>
                        <div style="display: flex; gap: 1.5rem; align-items: center;">
                            <a href="/">Home</a>
                            <a href="/projects">Projects</a>
                            <a href="/about">About</a>
                            <a href="/contact">Contact</a>
                        </div>
                    </nav>
                </header>
                <main>
            """);
        await RenderSlotAsync();
        WriteHtml("""
                </main>
                <footer style="border-top: 1px solid var(--color-border); padding: 2rem 0; text-align: center; color: var(--color-muted); font-size: 0.875rem;">
                    <div class="container">
                        <p style="margin-bottom: 0.5rem;">Built with Atoll &mdash; a .NET-native framework inspired by Astro.</p>
                        <div style="display: flex; justify-content: center; gap: 1.5rem;">
                            <a href="https://github.com">GitHub</a>
                            <a href="https://linkedin.com">LinkedIn</a>
                        </div>
                    </div>
                </footer>
            </body>
            </html>
            """);
    }
}

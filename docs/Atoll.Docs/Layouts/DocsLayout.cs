using Atoll.Components;
using Atoll.Content.Collections;
using Atoll.Docs.Components;

namespace Atoll.Docs.Layouts;

/// <summary>
/// The main layout for the Atoll documentation site. Provides the full HTML
/// shell with header, sidebar navigation, main content area, and footer.
/// </summary>
public sealed class DocsLayout : AtollComponent
{
    /// <summary>
    /// Gets or sets the page title. Used in the &lt;title&gt; element.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Atoll Docs";

    /// <summary>
    /// Gets or sets the collection query used to build the sidebar navigation.
    /// </summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current page slug, used to highlight the active nav item.
    /// Set automatically from the page's <c>Slug</c> route parameter.
    /// </summary>
    [Parameter]
    public string Slug { get; set; } = "";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<!DOCTYPE html>");
        WriteHtml("""
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>
            """);
        WriteText(Title);
        WriteHtml(" - Atoll Docs");
        WriteHtml("""
                </title>
                <style>
                    :root {
                        --color-bg: #ffffff;
                        --color-text: #1a1a2e;
                        --color-primary: #0f3460;
                        --color-accent: #e94560;
                        --color-muted: #6b7280;
                        --color-border: #e5e7eb;
                        --color-sidebar-bg: #f9fafb;
                        --color-active: #eff6ff;
                        --color-code-bg: #1e293b;
                        --color-code-text: #e2e8f0;
                        --sidebar-width: 16rem;
                    }
                    *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
                    body { font-family: system-ui, -apple-system, sans-serif; background: var(--color-bg); color: var(--color-text); line-height: 1.7; }
                    a { color: var(--color-primary); text-decoration: none; }
                    a:hover { color: var(--color-accent); text-decoration: underline; }

                    /* Header */
                    .site-header { border-bottom: 1px solid var(--color-border); padding: 0.75rem 1.5rem; display: flex; justify-content: space-between; align-items: center; position: sticky; top: 0; background: var(--color-bg); z-index: 10; }
                    .site-header a.brand { font-size: 1.25rem; font-weight: 700; color: var(--color-primary); }
                    .site-header a.brand:hover { color: var(--color-accent); text-decoration: none; }
                    .site-header nav { display: flex; gap: 1.5rem; align-items: center; }
                    .site-header nav a { font-size: 0.875rem; color: var(--color-muted); }
                    .site-header nav a:hover { color: var(--color-primary); text-decoration: none; }

                    /* Docs layout */
                    .docs-container { display: flex; min-height: calc(100vh - 3rem); }
                    .sidebar { width: var(--sidebar-width); flex-shrink: 0; border-right: 1px solid var(--color-border); padding: 1.5rem 1rem; background: var(--color-sidebar-bg); position: sticky; top: 3rem; height: calc(100vh - 3rem); overflow-y: auto; }
                    .sidebar h3 { font-size: 0.7rem; text-transform: uppercase; letter-spacing: 0.08em; color: var(--color-muted); margin: 1.25rem 0 0.5rem; font-weight: 600; }
                    .sidebar h3:first-child { margin-top: 0; }
                    .sidebar ul { list-style: none; }
                    .sidebar li a { display: block; padding: 0.3rem 0.75rem; border-radius: 0.25rem; font-size: 0.875rem; color: var(--color-text); transition: background 0.1s; }
                    .sidebar li a:hover { background: var(--color-active); color: var(--color-primary); text-decoration: none; }
                    .sidebar li a.active { background: var(--color-active); color: var(--color-primary); font-weight: 600; }

                    /* Content */
                    .content { flex: 1; padding: 2.5rem 3rem; min-width: 0; }
                    .content-inner { max-width: 48rem; }

                    /* Prose */
                    .prose h1 { font-size: 2rem; font-weight: 700; margin-bottom: 0.75rem; color: var(--color-primary); }
                    .prose h2 { font-size: 1.375rem; font-weight: 600; margin-top: 2.5rem; margin-bottom: 0.75rem; border-bottom: 1px solid var(--color-border); padding-bottom: 0.375rem; }
                    .prose h2:first-child { margin-top: 0; }
                    .prose h3 { font-size: 1.125rem; font-weight: 600; margin-top: 1.75rem; margin-bottom: 0.5rem; }
                    .prose p { margin-bottom: 1rem; }
                    .prose code { background: #f3f4f6; padding: 0.15rem 0.35rem; border-radius: 0.25rem; font-size: 0.875em; font-family: ui-monospace, monospace; color: #be123c; }
                    .prose pre { background: var(--color-code-bg); color: var(--color-code-text); padding: 1.25rem 1.5rem; border-radius: 0.5rem; overflow-x: auto; margin-bottom: 1.25rem; font-size: 0.875rem; line-height: 1.6; }
                    .prose pre code { background: none; padding: 0; color: inherit; font-size: inherit; }
                    .prose ul, .prose ol { margin-bottom: 1rem; padding-left: 1.5rem; }
                    .prose li { margin-bottom: 0.25rem; }
                    .prose table { width: 100%; border-collapse: collapse; margin-bottom: 1.25rem; font-size: 0.9rem; }
                    .prose th, .prose td { border: 1px solid var(--color-border); padding: 0.5rem 0.875rem; text-align: left; }
                    .prose th { background: var(--color-sidebar-bg); font-weight: 600; }
                    .prose a { color: var(--color-primary); text-decoration: underline; }
                    .prose a:hover { color: var(--color-accent); }
                    .prose blockquote { border-left: 3px solid var(--color-border); padding-left: 1rem; color: var(--color-muted); margin-bottom: 1rem; }

                    /* Index/landing page */
                    .hero { padding: 3rem 0 2rem; }
                    .hero h1 { font-size: 2.5rem; font-weight: 800; color: var(--color-primary); margin-bottom: 1rem; }
                    .hero .tagline { font-size: 1.125rem; color: var(--color-muted); margin-bottom: 2rem; max-width: 36rem; }
                    .hero .cta { display: inline-block; background: var(--color-primary); color: #fff; padding: 0.75rem 1.5rem; border-radius: 0.375rem; font-weight: 600; font-size: 0.9rem; }
                    .hero .cta:hover { background: var(--color-accent); text-decoration: none; }
                    .features { margin-top: 3rem; display: grid; grid-template-columns: repeat(auto-fill, minmax(14rem, 1fr)); gap: 1.25rem; }
                    .feature-card { border: 1px solid var(--color-border); border-radius: 0.5rem; padding: 1.25rem; background: var(--color-sidebar-bg); }
                    .feature-card h3 { font-size: 0.95rem; font-weight: 600; margin-bottom: 0.5rem; color: var(--color-primary); }
                    .feature-card p { font-size: 0.875rem; color: var(--color-muted); margin: 0; }

                    /* Footer */
                    .site-footer { border-top: 1px solid var(--color-border); padding: 1.25rem 1.5rem; text-align: center; color: var(--color-muted); font-size: 0.8rem; }

                    /* Responsive */
                    @media (max-width: 768px) {
                        .docs-container { flex-direction: column; }
                        .sidebar { width: 100%; height: auto; position: static; border-right: none; border-bottom: 1px solid var(--color-border); }
                        .content { padding: 1.5rem; }
                    }
                </style>
            </head>
            <body>
                <header class="site-header">
                    <a href="/" class="brand">Atoll</a>
                    <nav>
                        <a href="/">Docs</a>
                        <a href="https://github.com/damianh/atoll">GitHub</a>
                    </nav>
                </header>
                <div class="docs-container">
                    <aside class="sidebar">
            """);

        var sidebarProps = new Dictionary<string, object?>
        {
            ["Query"] = Query,
            ["CurrentSlug"] = Slug,
        };
        var sidebarFragment = ComponentRenderer.ToFragment<Sidebar>(sidebarProps);
        await RenderAsync(sidebarFragment);

        WriteHtml("""
                    </aside>
                    <main class="content">
                        <div class="content-inner">
            """);

        await RenderSlotAsync();

        WriteHtml("""
                        </div>
                    </main>
                </div>
                <footer class="site-footer">
                    <p>Built with <a href="https://github.com/damianh/atoll">Atoll</a> &mdash; a .NET-native framework inspired by Astro.</p>
                </footer>
            </body>
            </html>
            """);
    }
}

using Atoll.Components;

namespace AtollStarter.Layouts;

/// <summary>
/// The main layout. Wraps every page with the HTML document structure.
/// Edit this file to customize the site-wide header, footer, and styles.
/// </summary>
public sealed class MainLayout : AtollComponent
{
    /// <summary>
    /// Gets or sets the page title. Used in the &lt;title&gt; element.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "AtollStarter";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<!DOCTYPE html>");
        WriteHtml("<html lang=\"en\">");
        WriteHtml("<head>");
        WriteHtml("  <meta charset=\"UTF-8\" />");
        WriteHtml("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        WriteHtml("  <title>");
        WriteText(Title);
        WriteHtml("  </title>");
        WriteHtml("""
              <style>
                  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
                  body { font-family: system-ui, -apple-system, sans-serif; color: #1a1a2e; line-height: 1.7; }
                  a { color: #0f3460; text-decoration: none; }
                  a:hover { text-decoration: underline; }
                  .container { max-width: 48rem; margin: 0 auto; padding: 0 1.5rem; }
                  header { border-bottom: 1px solid #e0e0e0; padding: 1rem 0; }
                  footer { border-top: 1px solid #e0e0e0; padding: 1.5rem 0; text-align: center; color: #666; font-size: 0.875rem; margin-top: 4rem; }
              </style>
          </head>
          <body>
              <header>
                  <nav class="container" style="display: flex; justify-content: space-between; align-items: center;">
                      <a href="/" style="font-size: 1.25rem; font-weight: 700;">AtollStarter</a>
                      <div style="display: flex; gap: 1.5rem;">
                          <a href="/">Home</a>
                      </div>
                  </nav>
              </header>
              <main class="container" style="padding: 2rem 1.5rem;">
          """);
        await RenderSlotAsync();
        WriteHtml("""
                  </main>
                  <footer>
                      <div class="container">
                          <p>Built with <a href="https://github.com/example/atoll">Atoll</a> &mdash; a .NET-native framework.</p>
                      </div>
                  </footer>
              </body>
              </html>
          """);
    }
}

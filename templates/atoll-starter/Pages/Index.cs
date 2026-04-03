using Atoll.Components;
using Atoll.Routing;
using AtollStarter.Layouts;

namespace AtollStarter.Pages;

/// <summary>
/// The home page. Edit this file to customize your site's landing page.
/// </summary>
[Layout(typeof(MainLayout))]
[PageRoute("/")]
public sealed class Index : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <section style="text-align: center; padding: 4rem 0;">
                <h1 style="font-size: 2.5rem; margin-bottom: 1rem;">Welcome to AtollStarter</h1>
                <p style="font-size: 1.125rem; color: #666; max-width: 32rem; margin: 0 auto 2rem;">
                    Your new Atoll site is ready. Edit <code>Pages/Index.cs</code> to get started.
                </p>
                <div style="display: flex; gap: 1rem; justify-content: center; flex-wrap: wrap;">
                    <a href="https://github.com/example/atoll" style="display: inline-block; background: #0f3460; color: white; padding: 0.75rem 1.5rem; border-radius: 0.375rem; font-weight: 600;">
                        Read the Docs &rarr;
                    </a>
                </div>
            </section>
            """);
        return Task.CompletedTask;
    }
}

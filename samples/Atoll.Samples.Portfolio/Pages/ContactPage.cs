using Atoll.Core.Components;
using Atoll.Core.Rendering;
using Atoll.Routing;
using Atoll.Samples.Portfolio.Islands;
using Atoll.Samples.Portfolio.Layouts;

namespace Atoll.Samples.Portfolio.Pages;

/// <summary>
/// The contact page with an interactive contact form island.
/// Demonstrates the <see cref="ContactForm"/> island with <c>client:load</c>.
/// </summary>
[Layout(typeof(PortfolioLayout))]
public sealed class ContactPage : AtollComponent, IAtollPage
{
    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <section class="container" style="padding: 3rem 1.5rem; max-width: 40rem; margin: 0 auto;">
                <h1 style="font-size: 2.25rem; color: var(--color-heading); margin-bottom: 0.5rem;">Get in Touch</h1>
                <p style="color: var(--color-muted); margin-bottom: 2rem;">
                    Have a project in mind or just want to chat? Fill out the form below
                    and I'll get back to you as soon as possible.
                </p>
            """);

        // Contact form island (client:load)
        var formProps = new Dictionary<string, object?>
        {
            ["ActionUrl"] = "/api/contact",
        };
        var formFragment = ComponentRenderer.ToFragment<ContactForm>(formProps);
        await RenderAsync(formFragment);

        WriteHtml("""
                <div style="margin-top: 3rem; padding-top: 2rem; border-top: 1px solid var(--color-border);">
                    <h2 style="font-size: 1.25rem; color: var(--color-heading); margin-bottom: 1rem;">Other Ways to Reach Me</h2>
                    <div style="display: flex; flex-direction: column; gap: 0.75rem;">
                        <p style="color: var(--color-muted);">
                            <span style="color: var(--color-primary);">Email:</span> alex@example.com
                        </p>
                        <p style="color: var(--color-muted);">
                            <span style="color: var(--color-primary);">GitHub:</span>
                            <a href="https://github.com/example">github.com/example</a>
                        </p>
                        <p style="color: var(--color-muted);">
                            <span style="color: var(--color-primary);">LinkedIn:</span>
                            <a href="https://linkedin.com/in/example">linkedin.com/in/example</a>
                        </p>
                    </div>
                </div>
            </section>
            """);
    }
}

using Atoll.Components;
using Atoll.Islands;

namespace AtollPortfolio.Islands;

/// <summary>
/// An interactive contact form island component. Uses <c>client:load</c> for
/// immediate hydration so the form validation and submission are interactive on load.
/// Demonstrates the <see cref="ClientLoadAttribute"/> directive.
/// </summary>
[ClientLoad]
public sealed class ContactForm : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/contact-form.js";

    /// <summary>
    /// Gets or sets the form action URL for submission.
    /// </summary>
    [Parameter]
    public string ActionUrl { get; set; } = "/api/contact";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<form id=\"contact-form\" style=\"display: flex; flex-direction: column; gap: 1.25rem;\"");
        WriteHtml(" action=\"");
        WriteText(ActionUrl);
        WriteHtml("\" method=\"post\">");

        // Name field
        WriteHtml("""
            <div>
                <label for="contact-name" style="display: block; font-size: 0.875rem; color: var(--color-muted); margin-bottom: 0.375rem;">Name</label>
                <input type="text" id="contact-name" name="name" required
                    style="width: 100%; padding: 0.625rem 0.75rem; background: var(--color-bg); border: 1px solid var(--color-border); border-radius: 0.375rem; color: var(--color-text); font-size: 1rem;"
                    placeholder="Your name" />
            </div>
            """);

        // Email field
        WriteHtml("""
            <div>
                <label for="contact-email" style="display: block; font-size: 0.875rem; color: var(--color-muted); margin-bottom: 0.375rem;">Email</label>
                <input type="email" id="contact-email" name="email" required
                    style="width: 100%; padding: 0.625rem 0.75rem; background: var(--color-bg); border: 1px solid var(--color-border); border-radius: 0.375rem; color: var(--color-text); font-size: 1rem;"
                    placeholder="you@example.com" />
            </div>
            """);

        // Message field
        WriteHtml("""
            <div>
                <label for="contact-message" style="display: block; font-size: 0.875rem; color: var(--color-muted); margin-bottom: 0.375rem;">Message</label>
                <textarea id="contact-message" name="message" rows="5" required
                    style="width: 100%; padding: 0.625rem 0.75rem; background: var(--color-bg); border: 1px solid var(--color-border); border-radius: 0.375rem; color: var(--color-text); font-size: 1rem; resize: vertical;"
                    placeholder="Tell me about your project..."></textarea>
            </div>
            """);

        // Submit button
        WriteHtml("""
            <div>
                <button type="submit" class="btn-primary" style="border: none; cursor: pointer; font-size: 1rem; width: 100%;">
                    Send Message
                </button>
            </div>
            <div id="form-status" style="display: none; padding: 0.75rem; border-radius: 0.375rem; text-align: center;"></div>
            """);

        WriteHtml("</form>");
        return Task.CompletedTask;
    }
}

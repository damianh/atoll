using Atoll.Components;

namespace AtollBlog.Components;

/// <summary>
/// Renders a blog post card with title, date, description, and tag list.
/// Used on the blog index page.
/// </summary>
public sealed class PostCard : AtollComponent
{
    /// <summary>
    /// Gets or sets the post title.
    /// </summary>
    [Parameter(Required = true)]
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the post slug for the detail page link.
    /// </summary>
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    /// <summary>
    /// Gets or sets the post description.
    /// </summary>
    [Parameter]
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the publication date as a formatted string.
    /// </summary>
    [Parameter]
    public string Date { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("""
            <article style="border: 1px solid var(--color-border); border-radius: 0.5rem; padding: 1.5rem; margin-bottom: 1.5rem;">
            """);
        WriteHtml("<h2 style=\"margin-bottom: 0.25rem;\"><a href=\"/blog/");
        WriteText(Slug);
        WriteHtml("\">");
        WriteText(Title);
        WriteHtml("</a></h2>");

        if (!string.IsNullOrEmpty(Date))
        {
            WriteHtml("<time style=\"color: var(--color-muted); font-size: 0.875rem;\">");
            WriteText(Date);
            WriteHtml("</time>");
        }

        if (!string.IsNullOrEmpty(Description))
        {
            WriteHtml("<p style=\"margin-top: 0.5rem;\">");
            WriteText(Description);
            WriteHtml("</p>");
        }

        WriteHtml("</article>");
        return Task.CompletedTask;
    }
}

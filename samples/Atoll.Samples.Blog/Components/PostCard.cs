using Atoll.Components;

namespace Atoll.Samples.Blog.Components;

/// <summary>
/// Renders a blog post card with title, date, description, and tag list.
/// Used on the blog index page and tag-filtered listing pages.
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

    /// <summary>
    /// Gets or sets the comma-separated tag list.
    /// </summary>
    [Parameter]
    public string Tags { get; set; } = "";

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

        if (!string.IsNullOrEmpty(Tags))
        {
            WriteHtml("<div style=\"margin-top: 0.75rem; display: flex; gap: 0.5rem; flex-wrap: wrap;\">");
            var tags = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var tag in tags)
            {
                WriteHtml("<a href=\"/tags/");
                WriteText(tag.ToLowerInvariant());
                WriteHtml("\" style=\"background: var(--color-primary); color: white; padding: 0.125rem 0.5rem; border-radius: 0.25rem; font-size: 0.75rem;\">");
                WriteText(tag);
                WriteHtml("</a>");
            }

            WriteHtml("</div>");
        }

        WriteHtml("</article>");
        return Task.CompletedTask;
    }
}

using Atoll.Core.Components;

namespace Atoll.Samples.Blog.Components;

/// <summary>
/// Renders a tag badge as a styled inline link.
/// </summary>
public sealed class TagBadge : AtollComponent
{
    /// <summary>
    /// Gets or sets the tag name.
    /// </summary>
    [Parameter(Required = true)]
    public string Tag { get; set; } = "";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<a href=\"/tags/");
        WriteText(Tag.ToLowerInvariant());
        WriteHtml("\" style=\"display: inline-block; background: var(--color-primary); color: white; padding: 0.25rem 0.75rem; border-radius: 0.25rem; font-size: 0.875rem; margin: 0.25rem;\">");
        WriteText(Tag);
        WriteHtml("</a>");
        return Task.CompletedTask;
    }
}

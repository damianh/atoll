using Markdig;
using Markdig.Renderers;

namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Markdig extension that adds security attributes (<c>target="_blank"</c> and
/// <c>rel="noopener noreferrer"</c>) to external links in Markdown content.
/// Only links with absolute URLs beginning with <c>http://</c> or <c>https://</c>
/// are affected. Relative links and root-relative paths are left untouched.
/// </summary>
internal sealed class ExternalLinkExtension : IMarkdownExtension
{
    private readonly ExternalLinkOptions _options;

    internal ExternalLinkExtension(ExternalLinkOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    void IMarkdownExtension.Setup(MarkdownPipelineBuilder pipeline)
    {
        // No AST-level setup needed — we hook the renderer.
    }

    void IMarkdownExtension.Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is not HtmlRenderer htmlRenderer)
        {
            return;
        }

        LinkResolutionExtension.EnsureAtollLinkRenderer(htmlRenderer).ExternalLinks = _options;
    }
}

using Atoll.Build.Content.Markdown;
using Markdig;

namespace Atoll.Lagoon.Markdown;

/// <summary>
/// Extends the core <see cref="MarkdownRenderer"/> with docs-specific Markdig extensions,
/// specifically Mermaid diagram rendering.
/// </summary>
public static class DocsMarkdownRenderer
{
    /// <summary>
    /// Renders the specified Markdown content to HTML using the provided docs-specific options.
    /// When <see cref="DocsMarkdownOptions.EnableMermaid"/> is <c>true</c>, fenced code blocks
    /// with language <c>mermaid</c> are rendered as <c>&lt;pre class="mermaid"&gt;</c>.
    /// </summary>
    /// <param name="markdown">The Markdown content to render.</param>
    /// <param name="options">The docs-specific rendering options.</param>
    /// <returns>A <see cref="MarkdownRenderResult"/> containing HTML and heading metadata.</returns>
    public static MarkdownRenderResult Render(string markdown, DocsMarkdownOptions options)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(options);

        var pipeline = BuildPipeline(options);
        var document = Markdig.Markdown.Parse(markdown, pipeline);
        var html = document.ToHtml(pipeline);

        // Extract headings using the core renderer's heading-extraction path.
        var headingsResult = MarkdownRenderer.Render(markdown, options.Core);

        return new MarkdownRenderResult(html, headingsResult.Headings);
    }

    /// <summary>
    /// Builds a Markdig pipeline from the specified docs options, including any enabled
    /// docs-specific extensions.
    /// </summary>
    /// <param name="options">The docs-specific rendering options.</param>
    /// <returns>A configured <see cref="MarkdownPipeline"/>.</returns>
    public static MarkdownPipeline BuildPipeline(DocsMarkdownOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Start from the core pipeline.
        var corePipeline = MarkdownRenderer.BuildPipeline(options.Core);

        // If no docs-specific extensions needed, return the core pipeline directly.
        if (!options.EnableMermaid && !options.EnableSyntaxHighlighting)
        {
            return corePipeline;
        }

        // Rebuild with the Mermaid extension appended.
        var builder = new MarkdownPipelineBuilder();
        foreach (var ext in corePipeline.Extensions)
        {
            builder.Extensions.Add(ext);
        }

        if (options.EnableMermaid)
        {
            builder.Extensions.Add(new MermaidExtension());
        }

        if (options.EnableSyntaxHighlighting)
        {
            // SyntaxHighlightExtension must be added AFTER MermaidExtension so that
            // it captures MermaidCodeBlockRenderer as its fallback (when both are enabled).
            builder.Extensions.Add(new SyntaxHighlightExtension());
        }

        return builder.Build();
    }
}

using Atoll.Build.Content.Markdown;
using Atoll.Lagoon.Configuration;
using Atoll.Mermaid;
using Markdig;

namespace Atoll.Lagoon.Markdown;

/// <summary>
/// Extends the core <see cref="MarkdownRenderer"/> with docs-specific Markdig extensions
/// such as Mermaid diagram rendering and server-side syntax highlighting.
/// </summary>
public static class DocsMarkdownRenderer
{
    /// <summary>
    /// Builds a <see cref="MarkdownOptions"/> instance with Lagoon-specific extensions
    /// (Mermaid, syntax highlighting) populated in <see cref="MarkdownOptions.Extensions"/>
    /// based on the provided <see cref="DocsConfig"/>.
    /// Use this to configure a <see cref="Atoll.Build.Content.Collections.CollectionConfig"/>
    /// so that <c>CollectionQuery.Render()</c> applies Lagoon extensions.
    /// </summary>
    /// <param name="config">The documentation site configuration.</param>
    /// <returns>A <see cref="MarkdownOptions"/> with Lagoon extensions, or <c>null</c> if
    /// no Lagoon-specific extensions are enabled.</returns>
    public static MarkdownOptions? CreateMarkdownOptions(DocsConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var extensions = BuildExtensionList(config.EnableMermaid, config.EnableSyntaxHighlighting);

        if (extensions is null)
        {
            return null;
        }

        return new MarkdownOptions { Extensions = extensions };
    }

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

        // Populate the core options with Lagoon extensions so the core pipeline includes them.
        var coreOptions = options.Core;
        coreOptions.Extensions = BuildExtensionList(options.EnableMermaid, options.EnableSyntaxHighlighting);

        var pipeline = MarkdownRenderer.BuildPipeline(coreOptions);
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

        var coreOptions = options.Core;
        coreOptions.Extensions = BuildExtensionList(options.EnableMermaid, options.EnableSyntaxHighlighting);

        return MarkdownRenderer.BuildPipeline(coreOptions);
    }

    private static IReadOnlyList<IMarkdownExtension>? BuildExtensionList(
        bool enableMermaid,
        bool enableSyntaxHighlighting)
    {
        if (!enableMermaid && !enableSyntaxHighlighting)
        {
            return null;
        }

        var extensions = new List<IMarkdownExtension>();

        if (enableMermaid)
        {
            extensions.Add(new MermaidExtension());
        }

        if (enableSyntaxHighlighting)
        {
            // SyntaxHighlightExtension must be added AFTER MermaidExtension so that
            // it captures MermaidCodeBlockRenderer as its fallback (when both are enabled).
            extensions.Add(new SyntaxHighlightExtension());
        }

        return extensions;
    }
}

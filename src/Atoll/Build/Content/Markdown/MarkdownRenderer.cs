using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Represents a heading extracted from Markdown content, including its depth,
/// text content, and an optional anchor ID.
/// </summary>
public sealed class MarkdownHeading
{
    /// <summary>
    /// Initializes a new instance of <see cref="MarkdownHeading"/>.
    /// </summary>
    /// <param name="depth">The heading level (1-6).</param>
    /// <param name="text">The heading text content.</param>
    /// <param name="id">The heading's anchor ID, or <c>null</c> if auto-identifiers are disabled.</param>
    public MarkdownHeading(int depth, string text, string? id)
    {
        Depth = depth;
        Text = text;
        Id = id;
    }

    /// <summary>
    /// Gets the heading level (1 for h1, 2 for h2, etc.).
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Gets the heading text content.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the heading's anchor ID, or <c>null</c> if auto-identifiers are disabled.
    /// </summary>
    public string? Id { get; }
}

/// <summary>
/// The result of rendering a Markdown document, containing the HTML output
/// and extracted heading metadata.
/// </summary>
public sealed class MarkdownRenderResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="MarkdownRenderResult"/>.
    /// </summary>
    /// <param name="html">The rendered HTML string.</param>
    /// <param name="headings">The extracted headings from the document.</param>
    public MarkdownRenderResult(string html, IReadOnlyList<MarkdownHeading> headings)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(headings);
        Html = html;
        Headings = headings;
    }

    /// <summary>
    /// Gets the rendered HTML string.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the headings extracted from the document, in document order.
    /// </summary>
    public IReadOnlyList<MarkdownHeading> Headings { get; }
}

/// <summary>
/// Renders Markdown content to HTML using a Markdig pipeline with configurable extensions.
/// Also extracts heading metadata for table-of-contents generation.
/// </summary>
/// <remarks>
/// <para>
/// The renderer builds a Markdig <see cref="MarkdownPipeline"/> from the provided
/// <see cref="MarkdownOptions"/> (or sensible defaults), renders the Markdown body to HTML,
/// and extracts heading elements for metadata. This is the Atoll equivalent of Astro's
/// Markdown rendering through <c>@astrojs/markdown-remark</c>.
/// </para>
/// </remarks>
public static class MarkdownRenderer
{
    /// <summary>
    /// Renders the specified Markdown content to HTML using default options.
    /// </summary>
    /// <param name="markdown">The Markdown content to render.</param>
    /// <returns>A <see cref="MarkdownRenderResult"/> containing HTML and heading metadata.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="markdown"/> is <c>null</c>.</exception>
    public static MarkdownRenderResult Render(string markdown)
    {
        return Render(markdown, new MarkdownOptions());
    }

    /// <summary>
    /// Renders the specified Markdown content to HTML using the provided options.
    /// </summary>
    /// <param name="markdown">The Markdown content to render.</param>
    /// <param name="options">The rendering options controlling which extensions are enabled.</param>
    /// <returns>A <see cref="MarkdownRenderResult"/> containing HTML and heading metadata.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="markdown"/> or <paramref name="options"/> is <c>null</c>.
    /// </exception>
    public static MarkdownRenderResult Render(string markdown, MarkdownOptions options)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(options);

        var pipeline = BuildPipeline(options);
        var document = Markdig.Markdown.Parse(markdown, pipeline);

        var html = document.ToHtml(pipeline);
        var headings = ExtractHeadings(document);

        return new MarkdownRenderResult(html, headings);
    }

    /// <summary>
    /// Builds a Markdig pipeline from the specified options.
    /// </summary>
    /// <param name="options">The options controlling which extensions are enabled.</param>
    /// <returns>A configured <see cref="MarkdownPipeline"/>.</returns>
    public static MarkdownPipeline BuildPipeline(MarkdownOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var builder = new MarkdownPipelineBuilder();

        if (options.EnableTables)
        {
            builder.UsePipeTables();
        }

        if (options.EnableAutoLinks)
        {
            builder.UseAutoLinks();
        }

        if (options.EnableTaskLists)
        {
            builder.UseTaskLists();
        }

        if (options.EnableEmphasisExtras)
        {
            builder.UseEmphasisExtras();
        }

        if (options.EnableFootnotes)
        {
            builder.UseFootnotes();
        }

        if (options.EnableAutoIdentifiers)
        {
            builder.UseAutoIdentifiers();
        }

        if (options.LinkResolution is { } linkResolution)
        {
            builder.Extensions.Add(new LinkResolutionExtension(linkResolution));
        }

        if (options.ExternalLinks is { } externalLinks)
        {
            builder.Extensions.Add(new ExternalLinkExtension(externalLinks));
        }

        return builder.Build();
    }

    private static List<MarkdownHeading> ExtractHeadings(MarkdownDocument document)
    {
        var headings = new List<MarkdownHeading>();

        foreach (var block in document)
        {
            if (block is HeadingBlock headingBlock)
            {
                var text = ExtractInlineText(headingBlock.Inline);
                var id = headingBlock.TryGetAttributes()?.Id;
                headings.Add(new MarkdownHeading(headingBlock.Level, text, id));
            }
        }

        return headings;
    }

    private static string ExtractInlineText(ContainerInline? inline)
    {
        if (inline is null)
        {
            return "";
        }

        var text = new System.Text.StringBuilder();
        foreach (var child in inline)
        {
            if (child is LiteralInline literal)
            {
                text.Append(literal.Content);
            }
            else if (child is ContainerInline container)
            {
                text.Append(ExtractInlineText(container));
            }
        }

        return text.ToString();
    }
}

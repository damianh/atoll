using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text.RegularExpressions;

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
/// The result of rendering a Markdown document, containing the HTML output,
/// extracted heading metadata, and optional content fragments for component embedding.
/// </summary>
public sealed class MarkdownRenderResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="MarkdownRenderResult"/> with plain HTML.
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
    /// Initializes a new instance of <see cref="MarkdownRenderResult"/> with fragment data
    /// for documents containing embedded component directives.
    /// </summary>
    /// <param name="html">The rendered HTML string (with placeholder comments).</param>
    /// <param name="headings">The extracted headings from the document.</param>
    /// <param name="fragments">
    /// The sequence of HTML and component fragments produced by splitting the HTML on
    /// placeholder comments. When non-null, consumers should use fragments instead of
    /// <see cref="Html"/> to render component content inline.
    /// </param>
    public MarkdownRenderResult(
        string html,
        IReadOnlyList<MarkdownHeading> headings,
        IReadOnlyList<ContentFragment> fragments)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(headings);
        ArgumentNullException.ThrowIfNull(fragments);
        Html = html;
        Headings = headings;
        Fragments = fragments;
    }

    /// <summary>
    /// Gets the rendered HTML string.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the headings extracted from the document, in document order.
    /// </summary>
    public IReadOnlyList<MarkdownHeading> Headings { get; }

    /// <summary>
    /// Gets the content fragments produced when the Markdown contained embedded component
    /// directives. When <c>null</c>, no directives were present and <see cref="Html"/>
    /// contains the complete rendered output.
    /// </summary>
    public IReadOnlyList<ContentFragment>? Fragments { get; }
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
    // Matches placeholder comments emitted by ComponentDirectiveRenderer.
    // Pattern: <!--atoll:0-->, <!--atoll:1-->, etc.
    // Capture group 1 contains the numeric index.
    private static readonly Regex PlaceholderPattern =
        new(@"<!--atoll:(\d+)-->", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    // Matches placeholder comments emitted by ComponentTagPreprocessor.
    // Pattern: <!--atoll-tag:0-->, <!--atoll-tag:1-->, etc.
    // Capture group 1 contains the numeric index.
    private static readonly Regex TagPlaceholderPattern =
        new(@"<!--atoll-tag:(\d+)-->", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

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

        IReadOnlyList<ComponentReference> tagReferences = [];
        ComponentDirectiveExtension? directiveExtension = null;

        if (options.Components is not null)
        {
            // Step 1: Pre-process PascalCase component tags BEFORE Markdig parsing.
            // The preprocessor uses <!--atoll-tag:N--> placeholders to avoid colliding
            // with the :::directive extension's <!--atoll:N--> placeholders.
            var preprocessor = new ComponentTagPreprocessor(options.Components, options);
            (markdown, tagReferences) = preprocessor.Process(markdown);

            // Step 2: Set up the ::: directive extension for Markdig.
            directiveExtension = new ComponentDirectiveExtension(options.Components);
        }

        var pipeline = BuildPipeline(options, directiveExtension);
        var document = Markdig.Markdown.Parse(markdown, pipeline);

        var html = document.ToHtml(pipeline);
        var headings = ExtractHeadings(document);

        // Step 3: Merge references from both sources if either was active.
        var directiveReferences = directiveExtension?.CollectedReferences
            ?? (IReadOnlyList<ComponentReference>)[];

        var allReferences = MergeReferences(tagReferences, directiveReferences);

        if (allReferences.Count > 0)
        {
            // Step 4: Renumber placeholder indices so both sequences share a single
            // unified <!--atoll:N--> namespace:
            //   - Directive placeholders <!--atoll:K--> become <!--atoll:{M+K}-->
            //     (offset by M = number of tag references, so tag refs occupy 0..M-1)
            //   - Tag placeholders <!--atoll-tag:N--> are normalised to <!--atoll:N-->
            // Order matters: offset directives FIRST, then normalise tags (to avoid
            // a normalised tag placeholder being re-matched by the directive offset step).
            var m = tagReferences.Count;
            if (m > 0 && directiveReferences.Count > 0)
            {
                // Offset directive placeholders.
                html = PlaceholderPattern.Replace(html, match =>
                {
                    var k = int.Parse(match.Groups[1].Value);
                    return $"<!--atoll:{m + k}-->";
                });
            }

            if (m > 0)
            {
                // Normalise tag placeholders.
                html = TagPlaceholderPattern.Replace(html, match => $"<!--atoll:{match.Groups[1].Value}-->");
            }

            var fragments = BuildFragments(html, allReferences);
            return new MarkdownRenderResult(html, headings, fragments);
        }

        return new MarkdownRenderResult(html, headings);
    }

    /// <summary>
    /// Builds a Markdig pipeline from the specified options.
    /// </summary>
    /// <param name="options">The options controlling which extensions are enabled.</param>
    /// <returns>A configured <see cref="MarkdownPipeline"/>.</returns>
    public static MarkdownPipeline BuildPipeline(MarkdownOptions options)
    {
        return BuildPipeline(options, directiveExtension: null);
    }

    private static MarkdownPipeline BuildPipeline(
        MarkdownOptions options,
        ComponentDirectiveExtension? directiveExtension)
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

        if (directiveExtension is not null)
        {
            // UseCustomContainers and UseGenericAttributes must be registered on the builder
            // directly (before Build() is called), not inside the extension's Setup method.
            // Calling them inside Setup causes a "Collection was modified" error because
            // Markdig iterates the Extensions list during Build() while Setup is being called.
            builder.UseCustomContainers();
            builder.UseGenericAttributes();
            builder.Extensions.Add(directiveExtension);
        }

        // Append any additional extensions provided by addon packages (e.g. Atoll.Lagoon).
        if (options.Extensions is { Count: > 0 } extensions)
        {
            foreach (var extension in extensions)
            {
                builder.Extensions.Add(extension);
            }
        }

        return builder.Build();
    }

    /// <summary>
    /// Merges the tag-preprocessor references (indices 0..M-1) with the directive-extension
    /// references (indices M..M+K-1) into a single ordered list.
    /// </summary>
    private static IReadOnlyList<ComponentReference> MergeReferences(
        IReadOnlyList<ComponentReference> tagReferences,
        IReadOnlyList<ComponentReference> directiveReferences)
    {
        if (tagReferences.Count == 0)
        {
            return directiveReferences;
        }

        if (directiveReferences.Count == 0)
        {
            return tagReferences;
        }

        var merged = new List<ComponentReference>(tagReferences.Count + directiveReferences.Count);
        merged.AddRange(tagReferences);
        merged.AddRange(directiveReferences);
        return merged;
    }

    private static List<ContentFragment> BuildFragments(
        string html,
        IReadOnlyList<ComponentReference> references)
    {
        // Split HTML on <!--atoll:N--> placeholders.
        // Regex.Split with a capture group interleaves: [html, index, html, index, html, ...]
        var parts = PlaceholderPattern.Split(html);
        var fragments = new List<ContentFragment>(parts.Length);

        for (var i = 0; i < parts.Length; i++)
        {
            if (i % 2 == 0)
            {
                // Even positions are HTML chunks (may be empty).
                if (parts[i].Length > 0)
                {
                    fragments.Add(new HtmlContentFragment(parts[i]));
                }
            }
            else
            {
                // Odd positions are captured group values (the numeric index).
                if (int.TryParse(parts[i], out var index) && index < references.Count)
                {
                    fragments.Add(new ComponentContentFragment(references[index]));
                }
            }
        }

        return fragments;
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

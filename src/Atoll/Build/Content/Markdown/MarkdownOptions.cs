using Markdig;

namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Configuration options for the Markdig Markdown rendering pipeline.
/// Controls which CommonMark extensions are enabled and other rendering behavior.
/// </summary>
public sealed class MarkdownOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether pipe table support is enabled.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnableTables { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether automatic URL linking is enabled.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnableAutoLinks { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether task list (checkbox) support is enabled.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnableTaskLists { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether emphasis extras (strikethrough, etc.) are enabled.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnableEmphasisExtras { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether footnote support is enabled.
    /// Default: <c>false</c>.
    /// </summary>
    public bool EnableFootnotes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether heading IDs (automatic anchors) are enabled.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnableAutoIdentifiers { get; set; } = true;

    /// <summary>
    /// Gets or sets the CSS class to apply to fenced code blocks for syntax highlighting.
    /// The class will be set on the <c>&lt;code&gt;</c> element along with the language class.
    /// Set to <c>null</c> to disable.
    /// Default: <c>null</c>.
    /// </summary>
    public string? CodeBlockClass { get; set; }

    /// <summary>
    /// Gets or sets the link resolution options for rewriting relative Markdown file links to clean URL paths.
    /// When <c>null</c>, relative links are not rewritten.
    /// Default: <c>null</c>.
    /// </summary>
    public LinkResolutionOptions? LinkResolution { get; set; }

    /// <summary>
    /// Gets or sets the external link options for adding <c>target="_blank"</c> and
    /// <c>rel="noopener noreferrer"</c> to external links.
    /// When <c>null</c>, external links are not modified.
    /// Default: <c>null</c>.
    /// </summary>
    public ExternalLinkOptions? ExternalLinks { get; set; }

    /// <summary>
    /// Gets or sets the component map that enables <c>:::</c> directive syntax for embedding
    /// Atoll components inline within Markdown content.
    /// When <c>null</c>, component directives are not processed and Markdown renders
    /// identically to the current behavior.
    /// Default: <c>null</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set, the Markdig pipeline is extended with <c>CustomContainers</c> and
    /// <c>GenericAttributes</c> support. Any <c>:::name{key=value}</c> block whose
    /// <c>name</c> is registered in the map is replaced with the corresponding
    /// Atoll component during rendering.
    /// </para>
    /// <para>
    /// Example:
    /// <code>
    /// var options = new MarkdownOptions
    /// {
    ///     Components = new ComponentMap()
    ///         .Add&lt;Counter&gt;("counter")
    ///         .Add&lt;Callout&gt;("callout"),
    /// };
    /// </code>
    /// </para>
    /// </remarks>
    public ComponentMap? Components { get; set; }

    /// <summary>
    /// Gets or sets additional Markdig extensions to include in the rendering pipeline.
    /// These are appended after all built-in extensions, allowing addon packages
    /// (e.g. <c>Atoll.Lagoon</c>) to inject custom renderers such as syntax highlighting
    /// or diagram support.
    /// When <c>null</c> or empty, no additional extensions are added.
    /// Default: <c>null</c>.
    /// </summary>
    public IReadOnlyList<IMarkdownExtension>? Extensions { get; set; }
}

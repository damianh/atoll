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
}

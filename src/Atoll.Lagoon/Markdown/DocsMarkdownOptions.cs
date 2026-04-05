using Atoll.Build.Content.Markdown;

namespace Atoll.Lagoon.Markdown;

/// <summary>
/// Markdown options for documentation sites, combining the core
/// <see cref="MarkdownOptions"/> with docs-specific settings.
/// </summary>
public sealed class DocsMarkdownOptions
{
    /// <summary>
    /// Gets or sets the core Markdown rendering options.
    /// Defaults to a new <see cref="MarkdownOptions"/> instance with standard defaults.
    /// </summary>
    public MarkdownOptions Core { get; set; } = new MarkdownOptions();

    /// <summary>
    /// Gets or sets a value indicating whether Mermaid diagram rendering is enabled.
    /// When <c>true</c>, fenced code blocks with language <c>mermaid</c> are rendered as
    /// <c>&lt;pre class="mermaid"&gt;</c> instead of the default <c>&lt;pre&gt;&lt;code&gt;</c>.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool EnableMermaid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether server-side syntax highlighting is enabled.
    /// When <c>true</c>, fenced code blocks with recognized language identifiers are
    /// rendered with CSS classes for syntax coloring using TextMate grammars.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool EnableSyntaxHighlighting { get; set; }
}

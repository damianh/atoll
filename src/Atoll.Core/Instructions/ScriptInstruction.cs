using Atoll.Core.Rendering;

namespace Atoll.Core.Instructions;

/// <summary>
/// A render instruction that represents a script to be injected into the page.
/// Scripts can be either inline or referenced by URL.
/// </summary>
/// <remarks>
/// <para>
/// Script instructions are collected during rendering and deduplicated by their key.
/// Inline scripts are keyed by a scope identifier, while external scripts are keyed
/// by their URL. This ensures that the same script is not included more than once.
/// </para>
/// </remarks>
public sealed class ScriptInstruction : RenderInstruction
{
    private readonly RenderFragment _content;

    /// <summary>
    /// Initializes a new <see cref="ScriptInstruction"/> with the specified key and content.
    /// </summary>
    /// <param name="key">The deduplication key.</param>
    /// <param name="content">The script content as a <see cref="RenderFragment"/>.</param>
    public ScriptInstruction(string key, RenderFragment content)
        : base(key)
    {
        _content = content;
    }

    /// <summary>
    /// Gets a value indicating whether this is an inline script (vs. an external script reference).
    /// </summary>
    public bool IsInline { get; init; }

    /// <summary>
    /// Creates a <see cref="ScriptInstruction"/> for an external script reference.
    /// </summary>
    /// <param name="src">The script URL.</param>
    /// <returns>A new <see cref="ScriptInstruction"/>.</returns>
    public static ScriptInstruction External(string src)
    {
        ArgumentNullException.ThrowIfNull(src);
        var html = $"<script src=\"{HtmlEncoder.Encode(src)}\"></script>";
        return new ScriptInstruction(
            $"script:src:{src}",
            RenderFragment.FromHtml(html));
    }

    /// <summary>
    /// Creates a <see cref="ScriptInstruction"/> for an external script reference
    /// with the <c>type="module"</c> attribute.
    /// </summary>
    /// <param name="src">The script module URL.</param>
    /// <returns>A new <see cref="ScriptInstruction"/>.</returns>
    public static ScriptInstruction Module(string src)
    {
        ArgumentNullException.ThrowIfNull(src);
        var html = $"<script type=\"module\" src=\"{HtmlEncoder.Encode(src)}\"></script>";
        return new ScriptInstruction(
            $"script:module:{src}",
            RenderFragment.FromHtml(html));
    }

    /// <summary>
    /// Creates a <see cref="ScriptInstruction"/> for inline JavaScript content.
    /// </summary>
    /// <param name="scopeId">A unique scope identifier for deduplication.</param>
    /// <param name="javascript">The inline JavaScript content.</param>
    /// <returns>A new <see cref="ScriptInstruction"/>.</returns>
    public static ScriptInstruction Inline(string scopeId, string javascript)
    {
        ArgumentNullException.ThrowIfNull(scopeId);
        ArgumentNullException.ThrowIfNull(javascript);
        var html = $"<script>{javascript}</script>";
        return new ScriptInstruction(
            $"script:inline:{scopeId}",
            RenderFragment.FromHtml(html))
        {
            IsInline = true,
        };
    }

    /// <inheritdoc />
    public override ValueTask RenderAsync(IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return _content.RenderAsync(destination);
    }
}

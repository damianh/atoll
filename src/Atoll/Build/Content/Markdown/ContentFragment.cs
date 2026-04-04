namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Represents a fragment of rendered Markdown content — either a chunk of HTML
/// or a reference to an Atoll component to be rendered inline.
/// </summary>
/// <remarks>
/// <para>
/// When Markdown contains <c>:::</c> component directives, the rendered output is split
/// into a sequence of <see cref="ContentFragment"/> values. Each value is either an
/// <see cref="HtmlContentFragment"/> (raw HTML from Markdig) or a
/// <see cref="ComponentContentFragment"/> (an embedded component reference).
/// </para>
/// <para>
/// When no directives are present, <c>RenderedContent</c> retains a plain HTML string
/// and no fragments are produced — preserving the zero-overhead fast path.
/// </para>
/// </remarks>
public abstract class ContentFragment
{
}

/// <summary>
/// A <see cref="ContentFragment"/> containing raw HTML to be written directly to the output.
/// </summary>
/// <param name="Html">The raw HTML string.</param>
public sealed class HtmlContentFragment(string Html) : ContentFragment
{
    /// <summary>Gets the raw HTML string.</summary>
    public string Html { get; } = Html;
}

/// <summary>
/// A <see cref="ContentFragment"/> representing an Atoll component to be rendered inline.
/// </summary>
/// <param name="Reference">The component reference describing the type, props, and child HTML.</param>
public sealed class ComponentContentFragment(ComponentReference Reference) : ContentFragment
{
    /// <summary>Gets the component reference.</summary>
    public ComponentReference Reference { get; } = Reference;
}

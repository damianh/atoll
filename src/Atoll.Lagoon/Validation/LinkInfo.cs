namespace Atoll.Lagoon.Validation;

/// <summary>
/// Represents a single hyperlink extracted from a rendered page.
/// </summary>
public sealed class LinkInfo
{
    /// <summary>
    /// Initializes a new instance of <see cref="LinkInfo"/>.
    /// </summary>
    /// <param name="href">The raw <c>href</c> attribute value.</param>
    /// <param name="sourcePage">The URL path of the page that contains this link.</param>
    /// <param name="path">The URL path portion of the link, without any fragment.</param>
    /// <param name="fragment">The fragment (anchor) portion of the link, without the leading <c>#</c>, or <c>null</c> if absent.</param>
    /// <param name="kind">The classification of this link.</param>
    public LinkInfo(string href, string sourcePage, string path, string? fragment, LinkKind kind)
    {
        ArgumentNullException.ThrowIfNull(href);
        ArgumentNullException.ThrowIfNull(sourcePage);
        ArgumentNullException.ThrowIfNull(path);
        Href = href;
        SourcePage = sourcePage;
        Path = path;
        Fragment = fragment;
        Kind = kind;
    }

    /// <summary>Gets the raw <c>href</c> attribute value as it appears in the HTML.</summary>
    public string Href { get; }

    /// <summary>Gets the URL path of the page that contains this link.</summary>
    public string SourcePage { get; }

    /// <summary>Gets the URL path portion of the link, without any fragment.</summary>
    public string Path { get; }

    /// <summary>
    /// Gets the fragment (anchor) portion of the link, without the leading <c>#</c>,
    /// or <c>null</c> if the link has no fragment.
    /// </summary>
    public string? Fragment { get; }

    /// <summary>Gets the classification of this link.</summary>
    public LinkKind Kind { get; }

    /// <summary>
    /// Gets a value indicating whether this link targets a page within the same site
    /// (i.e. the href starts with <c>/</c>).
    /// </summary>
    public bool IsInternal => Kind == LinkKind.Internal;
}

/// <summary>
/// Classifies a hyperlink by its target type.
/// </summary>
public enum LinkKind
{
    /// <summary>An internal site link whose <c>href</c> starts with <c>/</c>.</summary>
    Internal,

    /// <summary>An external link whose <c>href</c> starts with <c>http://</c> or <c>https://</c>.</summary>
    External,

    /// <summary>A fragment-only link (e.g. <c>#section</c>) resolved against the source page.</summary>
    SamePageFragment,

    /// <summary>A non-navigable link such as <c>mailto:</c>, <c>tel:</c>, <c>javascript:</c>, or empty.</summary>
    Other,
}

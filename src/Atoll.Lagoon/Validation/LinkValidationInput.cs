namespace Atoll.Lagoon.Validation;

/// <summary>
/// Represents a single page to be validated by the <see cref="LagoonLinkValidator"/>.
/// Callers construct these from content entries and their resolved URLs.
/// </summary>
public sealed class LinkValidationInput
{
    /// <summary>
    /// Initializes a new instance of <see cref="LinkValidationInput"/>.
    /// </summary>
    /// <param name="urlPath">The URL path of this page (e.g. <c>/docs/getting-started/</c>).</param>
    /// <param name="anchorIds">
    /// The IDs of heading anchors present on this page (without leading <c>#</c>).
    /// </param>
    /// <param name="html">The rendered HTML body of the page, used to extract outbound links.</param>
    public LinkValidationInput(string urlPath, IReadOnlyList<string> anchorIds, string html)
    {
        ArgumentNullException.ThrowIfNull(urlPath);
        ArgumentNullException.ThrowIfNull(anchorIds);
        ArgumentNullException.ThrowIfNull(html);
        UrlPath = urlPath;
        AnchorIds = anchorIds;
        Html = html;
    }

    /// <summary>Gets the URL path of this page (e.g. <c>/docs/getting-started/</c>).</summary>
    public string UrlPath { get; }

    /// <summary>
    /// Gets the IDs of heading anchors present on this page (without leading <c>#</c>).
    /// </summary>
    public IReadOnlyList<string> AnchorIds { get; }

    /// <summary>
    /// Gets the rendered HTML body of the page. The validator parses this HTML to
    /// discover outbound <c>&lt;a href&gt;</c> links.
    /// </summary>
    public string Html { get; }
}

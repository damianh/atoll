using AngleSharp.Html.Parser;

namespace Atoll.Lagoon.Validation;

/// <summary>
/// Extracts <see cref="LinkInfo"/> instances from rendered HTML by parsing
/// all <c>&lt;a href&gt;</c> elements.
/// </summary>
public static class LinkExtractor
{
    private static readonly IHtmlParser Parser = new HtmlParser();

    /// <summary>
    /// Extracts all hyperlinks from the given HTML fragment.
    /// Fragment-only links (e.g. <c>#section</c>) are resolved against the source page path.
    /// Anchors without an <c>href</c> attribute are skipped.
    /// </summary>
    /// <param name="html">The rendered HTML to extract links from.</param>
    /// <param name="sourcePagePath">
    /// The URL path of the page this HTML was rendered from (e.g. <c>/docs/getting-started/</c>).
    /// Used to resolve fragment-only links.
    /// </param>
    /// <returns>A read-only list of extracted <see cref="LinkInfo"/> objects.</returns>
    public static IReadOnlyList<LinkInfo> Extract(string html, string sourcePagePath)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(sourcePagePath);

        if (html.Length == 0)
        {
            return [];
        }

        var document = Parser.ParseDocument(html);
        var anchors = document.QuerySelectorAll("a[href]");

        var results = new List<LinkInfo>(anchors.Length);

        foreach (var anchor in anchors)
        {
            var href = anchor.GetAttribute("href");
            if (href is null || href.Length == 0)
            {
                continue;
            }

            var link = Classify(href, sourcePagePath);
            results.Add(link);
        }

        return results;
    }

    private static LinkInfo Classify(string href, string sourcePagePath)
    {
        // Fragment-only links: "#section"
        if (href.StartsWith('#'))
        {
            var fragment = href[1..]; // strip leading '#'
            var resolvedPath = NormalizeTrailingSlash(sourcePagePath);
            return new LinkInfo(href, sourcePagePath, resolvedPath, fragment, LinkKind.SamePageFragment);
        }

        // External links: "http://" or "https://"
        if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return new LinkInfo(href, sourcePagePath, href, null, LinkKind.External);
        }

        // Non-navigable schemes: mailto:, tel:, javascript:, etc.
        if (href.Contains(':'))
        {
            return new LinkInfo(href, sourcePagePath, href, null, LinkKind.Other);
        }

        // Internal absolute links starting with '/'
        if (href.StartsWith('/'))
        {
            var hashIndex = href.IndexOf('#');
            if (hashIndex >= 0)
            {
                var path = NormalizeTrailingSlash(href[..hashIndex]);
                var fragment = href[(hashIndex + 1)..];
                return new LinkInfo(href, sourcePagePath, path, fragment.Length > 0 ? fragment : null, LinkKind.Internal);
            }
            else
            {
                var path = NormalizeTrailingSlash(href);
                return new LinkInfo(href, sourcePagePath, path, null, LinkKind.Internal);
            }
        }

        // Everything else (relative paths without leading slash, data: URIs, etc.)
        return new LinkInfo(href, sourcePagePath, href, null, LinkKind.Other);
    }

    private static string NormalizeTrailingSlash(string path)
    {
        if (path.Length == 0)
        {
            return "/";
        }

        // Preserve paths that look like files (have an extension in the last segment)
        var lastSegment = path.LastIndexOf('/') is var idx and >= 0
            ? path[(idx + 1)..]
            : path;

        if (lastSegment.Contains('.'))
        {
            return path;
        }

        return path.EndsWith('/') ? path : path + "/";
    }
}

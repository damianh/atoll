using System.Diagnostics;
using Atoll.Build.Content.Collections;

namespace Atoll.Lagoon.Validation;

/// <summary>
/// Validates internal links across a set of rendered pages, detecting broken links and
/// invalid fragment targets.
/// </summary>
/// <remarks>
/// <para>
/// Usage:
/// <code>
/// var validator = new LagoonLinkValidator();
/// var result = validator.Validate(query, config);
/// if (!result.IsValid)
/// {
///     foreach (var error in result.Errors)
///         Console.WriteLine(error.Message);
/// }
/// </code>
/// </para>
/// <para>
/// External links (<c>http://</c> / <c>https://</c>) are intentionally skipped.
/// </para>
/// </remarks>
public sealed class LagoonLinkValidator
{
    /// <summary>
    /// Validates links by obtaining pages from the <paramref name="configuration"/>
    /// and checking all internal links against the full page registry.
    /// </summary>
    /// <param name="query">The content collection query used to supply pages.</param>
    /// <param name="configuration">
    /// The user-supplied configuration that enumerates pages and declares options.
    /// </param>
    /// <returns>A <see cref="LinkValidationResult"/> summarising any errors found.</returns>
    public LinkValidationResult Validate(CollectionQuery query, ILinkValidationConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(configuration);

        var pages = configuration.GetPages(query);
        var options = configuration.Options;
        return Validate(pages, options);
    }

    /// <summary>
    /// Validates links from a sequence of <see cref="LinkValidationInput"/> pages using
    /// the default <see cref="LinkValidationOptions"/>.
    /// </summary>
    /// <param name="pages">The pages to validate.</param>
    /// <returns>A <see cref="LinkValidationResult"/> summarising any errors found.</returns>
    public LinkValidationResult Validate(IEnumerable<LinkValidationInput> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);
        return Validate(pages, new LinkValidationOptions());
    }

    /// <summary>
    /// Validates links from a sequence of <see cref="LinkValidationInput"/> pages using
    /// the supplied <paramref name="options"/>.
    /// </summary>
    /// <param name="pages">The pages to validate.</param>
    /// <param name="options">Options controlling what is validated.</param>
    /// <returns>A <see cref="LinkValidationResult"/> summarising any errors found.</returns>
    public LinkValidationResult Validate(IEnumerable<LinkValidationInput> pages, LinkValidationOptions options)
    {
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(options);

        var sw = Stopwatch.StartNew();

        // Materialise the page list so we can iterate it twice:
        // once to build the registry and once to extract and check links.
        var pageList = pages as IReadOnlyList<LinkValidationInput> ?? pages.ToList();

        // Pass 1: build registry of all known pages and their anchors.
        var registry = new PageRegistry();
        foreach (var page in pageList)
        {
            registry.Register(page.UrlPath, page.AnchorIds);
        }

        // Pass 2: extract links from each page and validate them.
        var errors = new List<LinkValidationError>();
        var linksChecked = 0;

        foreach (var page in pageList)
        {
            var links = LinkExtractor.Extract(page.Html, page.UrlPath);

            foreach (var link in links)
            {
                if (!link.IsInternal && link.Kind != LinkKind.SamePageFragment)
                {
                    // External links and non-navigable links are not validated.
                    continue;
                }

                if (IsExcluded(link.Path, options))
                {
                    continue;
                }

                linksChecked++;

                if (!registry.PageExists(link.Path))
                {
                    errors.Add(new LinkValidationError(
                        link.SourcePage,
                        link.Href,
                        LinkErrorKind.BrokenLink,
                        $"Broken link: '{link.Href}' on page '{link.SourcePage}' — target page '{link.Path}' does not exist."));

                    continue;
                }

                if (options.ValidateFragments && link.Fragment is { Length: > 0 } fragment)
                {
                    if (!registry.AnchorExists(link.Path, fragment))
                    {
                        errors.Add(new LinkValidationError(
                            link.SourcePage,
                            link.Href,
                            LinkErrorKind.InvalidFragment,
                            $"Invalid fragment: '{link.Href}' on page '{link.SourcePage}' — anchor '#{fragment}' does not exist on '{link.Path}'."));
                    }
                }
            }
        }

        sw.Stop();
        return new LinkValidationResult(errors, pageList.Count, linksChecked, sw.Elapsed);
    }

    private static bool IsExcluded(string path, LinkValidationOptions options)
    {
        if (options.ExcludePatterns.Count == 0)
        {
            return false;
        }

        foreach (var pattern in options.ExcludePatterns)
        {
            if (path.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

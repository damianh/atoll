using System.Text.RegularExpressions;
using Atoll.Build.Content.Collections;

namespace Atoll.Lagoon.Search;

/// <summary>
/// Input descriptor for a single document to include in the search index.
/// Callers construct these from content entries and their resolved URLs.
/// </summary>
public sealed class SearchDocumentInput
{
    /// <summary>
    /// Initializes a new <see cref="SearchDocumentInput"/>.
    /// </summary>
    /// <param name="title">The document title.</param>
    /// <param name="href">The URL path for the document.</param>
    public SearchDocumentInput(string title, string href)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(href);
        Title = title;
        Href = href;
        Headings = [];
        Topics = [];
    }

    /// <summary>Gets or sets the document title.</summary>
    public string Title { get; set; }

    /// <summary>Gets or sets the URL path.</summary>
    public string Href { get; set; }

    /// <summary>Gets or sets an optional short description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets an optional section label.</summary>
    public string? Section { get; set; }

    /// <summary>Gets or sets the raw HTML body to strip for plain-text indexing.</summary>
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>Gets or sets pre-extracted headings. If empty, will be parsed from <see cref="HtmlBody"/>.</summary>
    public IReadOnlyList<string> Headings { get; set; }

    /// <summary>
    /// Gets or sets the topic labels for this document (e.g., <c>["IdentityServer", "Security"]</c>).
    /// When set, takes priority over <see cref="Section"/> for topic display and filtering.
    /// When empty, the builder will auto-seed topics from <see cref="Section"/> if it is set.
    /// </summary>
    public IReadOnlyList<string> Topics { get; set; } = [];

    /// <summary>Gets or sets a direct plain-text body override. When set, <see cref="HtmlBody"/> is ignored.</summary>
    public string? PlainBody { get; set; }

    /// <summary>Gets or sets the maximum plain-text body length. Defaults to 500 characters.</summary>
    public int MaxBodyLength { get; set; } = 500;

    /// <summary>
    /// Gets or sets a value indicating whether this document is a draft.
    /// When <c>true</c>, this document should be excluded from the search index.
    /// Callers are responsible for filtering draft documents before passing them to the generator.
    /// </summary>
    public bool Draft { get; set; }
}

/// <summary>
/// Build-time utility that generates a <see cref="SearchIndex"/> from a set of
/// <see cref="SearchDocumentInput"/> descriptors. Strips HTML tags to produce
/// plain-text excerpts and normalises whitespace.
/// </summary>
public sealed class SearchIndexBuilder
{
    private static readonly Regex HtmlTagRegex = new Regex("<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

    private readonly List<SearchDocumentInput> _documents = [];

    /// <summary>Adds a single document to the builder.</summary>
    /// <param name="document">The document input descriptor.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SearchIndexBuilder Add(SearchDocumentInput document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _documents.Add(document);
        return this;
    }

    /// <summary>
    /// Adds all entries from a content collection with a selector that maps each entry
    /// to a <see cref="SearchDocumentInput"/>.
    /// </summary>
    /// <typeparam name="TData">The content entry schema type.</typeparam>
    /// <param name="entries">The content entries.</param>
    /// <param name="selector">Maps each entry to a <see cref="SearchDocumentInput"/>.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SearchIndexBuilder AddCollection<TData>(
        IEnumerable<ContentEntry<TData>> entries,
        Func<ContentEntry<TData>, SearchDocumentInput> selector)
        where TData : class, new()
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(selector);
        foreach (var entry in entries)
        {
            _documents.Add(selector(entry));
        }

        return this;
    }

    /// <summary>
    /// Builds the <see cref="SearchIndex"/> from all added documents.
    /// </summary>
    /// <returns>A complete <see cref="SearchIndex"/>.</returns>
    public SearchIndex Build()
    {
        return Build(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Builds the <see cref="SearchIndex"/> from all added documents with a specified timestamp.
    /// </summary>
    /// <param name="generatedAt">The generation timestamp to embed in the index.</param>
    /// <returns>A complete <see cref="SearchIndex"/>.</returns>
    public SearchIndex Build(DateTimeOffset generatedAt)
    {
        var entries = _documents.Select(ToSearchEntry).ToList();
        return new SearchIndex(entries, generatedAt);
    }

    private SearchEntry ToSearchEntry(SearchDocumentInput doc)
    {
        var plainBody = doc.PlainBody ?? StripHtml(doc.HtmlBody);
        plainBody = Truncate(plainBody, doc.MaxBodyLength);

        var headings = doc.Headings.Count > 0
            ? doc.Headings
            : ExtractHeadingsFromHtml(doc.HtmlBody);

        // Auto-seed topics from Section when no explicit topics are provided.
        // This ensures existing callers using only Section get topic support for free.
        IReadOnlyList<string>? topics = null;
        if (doc.Topics.Count > 0)
        {
            topics = doc.Topics;
        }
        else if (doc.Section != null)
        {
            topics = [doc.Section];
        }

        return new SearchEntry(doc.Title, doc.Href, doc.Description, doc.Section, headings, plainBody, topics);
    }

    /// <summary>
    /// Strips HTML tags from the given HTML string, returning plain text.
    /// Collapses whitespace to single spaces.
    /// </summary>
    /// <param name="html">The HTML string to strip.</param>
    /// <returns>Plain text with all HTML tags removed.</returns>
    public static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        var text = HtmlTagRegex.Replace(html, " ");
        text = WhitespaceRegex.Replace(text, " ");
        return text.Trim();
    }

    private static string Truncate(string text, int maxLength)
    {
        if (text.Length <= maxLength) return text;
        return text[..maxLength];
    }

    private static IReadOnlyList<string> ExtractHeadingsFromHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return [];
        var headings = new List<string>();
        var matches = Regex.Matches(html, @"<h[1-6][^>]*>(.*?)</h[1-6]>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        foreach (Match m in matches)
        {
            var text = HtmlTagRegex.Replace(m.Groups[1].Value, "").Trim();
            if (!string.IsNullOrEmpty(text))
            {
                headings.Add(text);
            }
        }

        return headings;
    }
}

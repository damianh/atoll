namespace Atoll.Lagoon.Redirects;

/// <summary>
/// Describes a single URL redirect rule: an old path (<see cref="From"/>) that should redirect
/// to a new path or URL (<see cref="To"/>) with a given HTTP status code.
/// </summary>
/// <remarks>
/// Use <see cref="RedirectRule(string, string)"/> for permanent 301 redirects (the most common case),
/// or <see cref="RedirectRule(string, string, int)"/> to specify 302 for temporary redirects.
/// </remarks>
public readonly record struct RedirectRule
{
    /// <summary>
    /// Gets the source URL path (the old URL that should be redirected).
    /// Must begin with a <c>/</c> for site-relative paths.
    /// </summary>
    public string From { get; }

    /// <summary>
    /// Gets the destination URL path or full URL (the new location).
    /// </summary>
    public string To { get; }

    /// <summary>
    /// Gets the HTTP status code for the redirect. Typically <c>301</c> (permanent) or
    /// <c>302</c> (temporary).
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Initializes a new permanent (301) redirect rule.
    /// </summary>
    /// <param name="from">The source URL path to redirect from.</param>
    /// <param name="to">The destination URL path or URL to redirect to.</param>
    public RedirectRule(string from, string to)
        : this(from, to, 301)
    {
    }

    /// <summary>
    /// Initializes a redirect rule with an explicit HTTP status code.
    /// </summary>
    /// <param name="from">The source URL path to redirect from.</param>
    /// <param name="to">The destination URL path or URL to redirect to.</param>
    /// <param name="statusCode">The HTTP status code (<c>301</c> for permanent, <c>302</c> for temporary).</param>
    public RedirectRule(string from, string to, int statusCode)
    {
        From = from;
        To = to;
        StatusCode = statusCode;
    }
}

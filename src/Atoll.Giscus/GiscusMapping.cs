namespace Atoll.Giscus;

/// <summary>
/// Specifies how giscus maps pages to GitHub Discussions.
/// </summary>
public enum GiscusMapping
{
    /// <summary>
    /// The discussion title contains the page pathname (e.g., <c>/blog/my-post</c>).
    /// This is the recommended default.
    /// </summary>
    Pathname,

    /// <summary>
    /// The discussion title contains the full page URL.
    /// </summary>
    Url,

    /// <summary>
    /// The discussion title contains the page <c>&lt;title&gt;</c> tag text.
    /// </summary>
    Title,

    /// <summary>
    /// The discussion title contains the <c>og:title</c> meta tag value.
    /// </summary>
    OgTitle,

    /// <summary>
    /// The discussion is looked up or created using a custom search term
    /// specified via <see cref="GiscusComments.Term"/>.
    /// </summary>
    Specific,

    /// <summary>
    /// A specific discussion is referenced by its number, specified via
    /// <see cref="GiscusComments.Term"/>. Discussions are not created automatically.
    /// </summary>
    Number,
}

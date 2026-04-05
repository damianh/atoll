namespace Atoll.Reef.Configuration;

/// <summary>
/// Describes a known author for the Reef theme.
/// Keyed by author identifier (matching the <c>Author</c> field in <see cref="ArticleSchema"/>).
/// </summary>
public sealed class AuthorInfo
{
    /// <summary>Gets or sets the author's display name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the URL or path to the author's avatar image.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets a short bio for the author.</summary>
    public string? Bio { get; set; }

    /// <summary>Gets or sets the URL of the author's personal site or profile page.</summary>
    public string? Url { get; set; }
}

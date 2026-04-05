namespace Atoll.Reef.Components;

/// <summary>
/// Represents a navigational link to an adjacent article (previous or next).
/// </summary>
public sealed class ArticleNavLink
{
    /// <summary>Initialises a new <see cref="ArticleNavLink"/>.</summary>
    /// <param name="title">The display title of the linked article.</param>
    /// <param name="href">The URL href to the linked article.</param>
    public ArticleNavLink(string title, string href)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        Title = title;
        Href = href;
    }

    /// <summary>Gets the display title of the linked article.</summary>
    public string Title { get; }

    /// <summary>Gets the URL href to the linked article.</summary>
    public string Href { get; }
}

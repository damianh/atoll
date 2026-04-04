namespace Atoll.Lagoon.Navigation;

/// <summary>
/// A navigable link in pagination (previous or next page).
/// </summary>
public sealed class PaginationLink
{
    /// <summary>
    /// Initializes a new instance of <see cref="PaginationLink"/>.
    /// </summary>
    /// <param name="label">The display label for the link.</param>
    /// <param name="href">The URL this link points to.</param>
    public PaginationLink(string label, string href)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(href);
        Label = label;
        Href = href;
    }

    /// <summary>Gets the display label.</summary>
    public string Label { get; }

    /// <summary>Gets the URL.</summary>
    public string Href { get; }
}

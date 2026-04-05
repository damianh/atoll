namespace Atoll.Lagoon.Configuration;

/// <summary>
/// Represents a single link to include in the custom site footer.
/// </summary>
/// <param name="Label">The display text for the link.</param>
/// <param name="Href">The URL the link points to.</param>
public sealed record FooterLink(string Label, string Href);

/// <summary>
/// Configuration for a custom site footer, replacing the default "Built with Atoll" footer.
/// </summary>
public sealed class FooterConfig
{
    /// <summary>
    /// Gets or sets optional raw HTML text to render in the footer.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets optional navigation links to render in the footer.
    /// </summary>
    public IReadOnlyList<FooterLink> Links { get; set; } = [];
}

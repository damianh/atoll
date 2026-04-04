namespace Atoll.Lagoon.Configuration;

/// <summary>
/// Defines the available social platform icons for use in <see cref="SocialLink"/>.
/// </summary>
public enum SocialIcon
{
    /// <summary>GitHub</summary>
    GitHub,

    /// <summary>Discord</summary>
    Discord,

    /// <summary>Twitter / X</summary>
    Twitter,

    /// <summary>Mastodon</summary>
    Mastodon,

    /// <summary>LinkedIn</summary>
    LinkedIn,

    /// <summary>YouTube</summary>
    YouTube,

    /// <summary>RSS feed</summary>
    Rss,

    /// <summary>Generic external link</summary>
    ExternalLink,
}

/// <summary>
/// Represents a social / external link shown in the docs site header or footer.
/// </summary>
public sealed class SocialLink
{
    /// <summary>
    /// Initializes a new instance of <see cref="SocialLink"/>.
    /// </summary>
    /// <param name="label">Accessible label for the link (e.g., "GitHub").</param>
    /// <param name="url">The URL the link points to.</param>
    /// <param name="icon">The icon to display alongside the label.</param>
    public SocialLink(string label, string url, SocialIcon icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        Label = label;
        Url = url;
        Icon = icon;
    }

    /// <summary>Gets the accessible label for the link.</summary>
    public string Label { get; }

    /// <summary>Gets the URL the link points to.</summary>
    public string Url { get; }

    /// <summary>Gets the icon to display alongside the label.</summary>
    public SocialIcon Icon { get; }
}

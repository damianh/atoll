namespace Atoll.Lagoon.Configuration;

/// <summary>
/// Configuration for a site-wide announcement banner displayed above the page content.
/// Set <see cref="DocsConfig.Banner"/> to an instance of this class to enable the banner.
/// </summary>
public sealed class BannerConfig
{
    /// <summary>
    /// Gets or sets the raw HTML content to display in the banner.
    /// Rendered as raw HTML (caller is responsible for encoding any user-supplied text).
    /// Default: <c>""</c> (no content — banner will not render).
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// Gets or sets the visual style variant of the banner.
    /// Each variant maps to the corresponding <c>--aside-*</c> CSS custom property set.
    /// Default: <see cref="BannerVariant.Info"/>.
    /// </summary>
    public BannerVariant Variant { get; set; } = BannerVariant.Info;

    /// <summary>
    /// Gets or sets an optional URL for a call-to-action link rendered inside the banner.
    /// When <c>null</c>, no link is rendered.
    /// Default: <c>null</c>.
    /// </summary>
    public string? LinkHref { get; set; }

    /// <summary>
    /// Gets or sets the display text for the optional CTA link.
    /// Only rendered when both <see cref="LinkHref"/> and <see cref="LinkText"/> are non-null.
    /// Default: <c>null</c>.
    /// </summary>
    public string? LinkText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the banner can be dismissed by the user.
    /// When <c>true</c>, a dismiss button is rendered and the dismissal state is persisted
    /// to <c>localStorage</c> using <see cref="DismissKey"/>.
    /// Default: <c>true</c>.
    /// </summary>
    public bool Dismissible { get; set; } = true;

    /// <summary>
    /// Gets or sets the <c>localStorage</c> key used to persist the banner's dismissal state.
    /// Changing this key resets all previously dismissed banners, which is useful for
    /// cache-busting when the banner message is updated.
    /// Default: <c>"atoll-banner-dismissed"</c>.
    /// </summary>
    public string DismissKey { get; set; } = "atoll-banner-dismissed";

    /// <summary>
    /// Gets or sets a value indicating whether the banner is enabled.
    /// When <c>false</c>, no banner HTML is rendered regardless of other settings.
    /// Default: <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

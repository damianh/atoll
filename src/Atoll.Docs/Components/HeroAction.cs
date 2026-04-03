namespace Atoll.Docs.Components;

/// <summary>
/// Variant style for a <see cref="HeroAction"/> button.
/// </summary>
public enum HeroActionVariant
{
    /// <summary>Primary call-to-action button (filled/prominent style).</summary>
    Primary,

    /// <summary>Secondary action button (outlined/subdued style).</summary>
    Secondary
}

/// <summary>
/// A call-to-action button displayed in a <see cref="Hero"/> component.
/// </summary>
public sealed class HeroAction
{
    /// <summary>
    /// Initializes a new instance of <see cref="HeroAction"/> with primary variant.
    /// </summary>
    /// <param name="label">The button label text.</param>
    /// <param name="href">The URL the button links to.</param>
    public HeroAction(string label, string href)
        : this(label, href, HeroActionVariant.Primary)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="HeroAction"/>.
    /// </summary>
    /// <param name="label">The button label text.</param>
    /// <param name="href">The URL the button links to.</param>
    /// <param name="variant">The visual variant of the button.</param>
    public HeroAction(string label, string href, HeroActionVariant variant)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(href);
        Label = label;
        Href = href;
        Variant = variant;
    }

    /// <summary>Gets the button label text.</summary>
    public string Label { get; }

    /// <summary>Gets the URL the button links to.</summary>
    public string Href { get; }

    /// <summary>Gets the visual variant of the button.</summary>
    public HeroActionVariant Variant { get; }
}

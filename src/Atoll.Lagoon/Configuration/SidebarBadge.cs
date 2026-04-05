namespace Atoll.Lagoon.Configuration;

/// <summary>
/// Represents a badge attached to a sidebar item, with optional colour variant.
/// Assignable from a plain <see cref="string"/> via implicit conversion for backward compatibility.
/// </summary>
public sealed record SidebarBadge
{
    /// <summary>
    /// Initializes a new <see cref="SidebarBadge"/> with the given text and variant.
    /// </summary>
    /// <param name="text">The badge display text.</param>
    /// <param name="variant">The colour variant. Defaults to <see cref="BadgeVariant.Default"/>.</param>
    public SidebarBadge(string text, BadgeVariant variant)
    {
        ArgumentNullException.ThrowIfNull(text);
        Text = text;
        Variant = variant;
    }

    /// <summary>
    /// Initializes a new <see cref="SidebarBadge"/> with the given text and the default colour variant.
    /// </summary>
    /// <param name="text">The badge display text.</param>
    public SidebarBadge(string text)
        : this(text, BadgeVariant.Default)
    {
    }

    /// <summary>Gets the badge display text.</summary>
    public string Text { get; init; }

    /// <summary>Gets the colour variant.</summary>
    public BadgeVariant Variant { get; init; }

    /// <summary>
    /// Implicitly converts a plain <see cref="string"/> to a <see cref="SidebarBadge"/>
    /// with <see cref="BadgeVariant.Default"/>, so that existing code like
    /// <c>Badge = "New"</c> continues to compile without changes.
    /// </summary>
    /// <param name="text">The badge text.</param>
    public static implicit operator SidebarBadge(string text) => new(text);
}

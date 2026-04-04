namespace Atoll.Lagoon.Navigation;

/// <summary>
/// A single item in a breadcrumb trail.
/// </summary>
public sealed class BreadcrumbItem
{
    /// <summary>
    /// Initializes a breadcrumb item that links to a page.
    /// </summary>
    /// <param name="label">The display label for this crumb.</param>
    /// <param name="href">The URL this crumb links to. Pass <c>null</c> for the current page.</param>
    /// <param name="isCurrent">Whether this is the current (last) page in the trail.</param>
    public BreadcrumbItem(string label, string? href, bool isCurrent)
    {
        ArgumentNullException.ThrowIfNull(label);
        Label = label;
        Href = href;
        IsCurrent = isCurrent;
    }

    /// <summary>Gets the display label.</summary>
    public string Label { get; }

    /// <summary>
    /// Gets the URL for this breadcrumb, or <c>null</c> when this is the current page.
    /// </summary>
    public string? Href { get; }

    /// <summary>Gets whether this is the current (last) page in the breadcrumb trail.</summary>
    public bool IsCurrent { get; }
}

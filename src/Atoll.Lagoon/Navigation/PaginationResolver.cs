namespace Atoll.Lagoon.Navigation;

/// <summary>
/// Resolves the previous and next page navigation links from the sidebar order.
/// </summary>
/// <remarks>
/// <para>
/// The resolver works by flattening the sidebar tree to a list of link items (using
/// <see cref="SidebarBuilder.Flatten"/>), then finding the current page by href and
/// returning its immediate neighbours.
/// </para>
/// <para>
/// Group headers (non-link items) are automatically excluded from the flat list.
/// </para>
/// </remarks>
public sealed class PaginationResolver
{
    private readonly IReadOnlyList<ResolvedSidebarItem> _flatItems;

    /// <summary>
    /// Initializes a new instance of <see cref="PaginationResolver"/> from a flat
    /// (already-flattened) ordered list of sidebar link items.
    /// </summary>
    /// <param name="flatItems">
    /// The ordered flat list of link items (no group headers).
    /// Obtain this by calling <see cref="SidebarBuilder.Flatten"/> on resolved sidebar items.
    /// </param>
    public PaginationResolver(IReadOnlyList<ResolvedSidebarItem> flatItems)
    {
        ArgumentNullException.ThrowIfNull(flatItems);
        _flatItems = flatItems;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PaginationResolver"/> from a tree of
    /// resolved sidebar items. The tree is flattened automatically.
    /// </summary>
    /// <param name="sidebarItems">
    /// The resolved sidebar items (may be nested). Group headers are excluded.
    /// </param>
    /// <param name="flatten">
    /// Pass <c>true</c> to flatten <paramref name="sidebarItems"/> automatically.
    /// This parameter exists to distinguish this overload from the flat-items constructor.
    /// </param>
    public PaginationResolver(IReadOnlyList<ResolvedSidebarItem> sidebarItems, bool flatten)
    {
        ArgumentNullException.ThrowIfNull(sidebarItems);
        _ = flatten; // parameter distinguishes this overload from the flat-items overload
        _flatItems = SidebarBuilder.Flatten(sidebarItems);
    }

    /// <summary>
    /// Resolves pagination for the given current page href.
    /// </summary>
    /// <param name="currentHref">
    /// The href of the current page (e.g., <c>"/docs/guides/start"</c>).
    /// Matched case-insensitively with trailing slash normalization.
    /// </param>
    /// <returns>
    /// A <see cref="PaginationResult"/> containing the previous and next links.
    /// Either may be <c>null</c> if at the edges of the list.
    /// </returns>
    public PaginationResult Resolve(string currentHref)
    {
        ArgumentNullException.ThrowIfNull(currentHref);

        var normalizedCurrent = currentHref.TrimEnd('/');
        var index = -1;

        for (var i = 0; i < _flatItems.Count; i++)
        {
            var itemHref = _flatItems[i].Href?.TrimEnd('/') ?? "";
            if (string.Equals(itemHref, normalizedCurrent, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            // Current page not found in sidebar — no pagination.
            return new PaginationResult(null, null);
        }

        var previous = index > 0
            ? new PaginationLink(_flatItems[index - 1].Label, _flatItems[index - 1].Href!)
            : null;

        var next = index < _flatItems.Count - 1
            ? new PaginationLink(_flatItems[index + 1].Label, _flatItems[index + 1].Href!)
            : null;

        return new PaginationResult(previous, next);
    }
}

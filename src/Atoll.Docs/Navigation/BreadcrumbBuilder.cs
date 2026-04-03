namespace Atoll.Docs.Navigation;

/// <summary>
/// Builds a breadcrumb trail by walking the sidebar tree to find the path
/// from root to the current page.
/// </summary>
/// <remarks>
/// <para>
/// The builder traverses the resolved sidebar tree depth-first, collecting group labels
/// as ancestor crumbs. When the current page is found, the accumulated ancestor path
/// plus the current page label form the breadcrumb trail.
/// </para>
/// <para>
/// Group headers are included as non-linkable ancestor crumbs (no <c>Href</c>).
/// The current page crumb has <see cref="BreadcrumbItem.IsCurrent"/> set to <c>true</c>
/// and <see cref="BreadcrumbItem.Href"/> set to <c>null</c>.
/// </para>
/// </remarks>
public sealed class BreadcrumbBuilder
{
    private readonly IReadOnlyList<ResolvedSidebarItem> _sidebar;

    /// <summary>
    /// Initializes a new instance of <see cref="BreadcrumbBuilder"/>.
    /// </summary>
    /// <param name="sidebar">The resolved sidebar tree to search.</param>
    public BreadcrumbBuilder(IReadOnlyList<ResolvedSidebarItem> sidebar)
    {
        ArgumentNullException.ThrowIfNull(sidebar);
        _sidebar = sidebar;
    }

    /// <summary>
    /// Builds the breadcrumb trail for the specified current href.
    /// </summary>
    /// <param name="currentHref">
    /// The href of the current page (e.g., <c>"/docs/guides/start"</c>).
    /// Matched case-insensitively with trailing slash normalization.
    /// </param>
    /// <returns>
    /// An ordered list of breadcrumb items from root to current page,
    /// or an empty list if the current page is not found in the sidebar.
    /// </returns>
    public IReadOnlyList<BreadcrumbItem> Build(string currentHref)
    {
        ArgumentNullException.ThrowIfNull(currentHref);

        var crumbs = new List<BreadcrumbItem>();
        TryBuild(_sidebar, currentHref, crumbs);
        return crumbs;
    }

    private static bool TryBuild(
        IReadOnlyList<ResolvedSidebarItem> items,
        string currentHref,
        List<BreadcrumbItem> accumulated)
    {
        foreach (var item in items)
        {
            if (item.IsGroup)
            {
                // Add the group as a non-linkable ancestor, then recurse.
                accumulated.Add(new BreadcrumbItem(item.Label, null, false));

                if (TryBuild(item.Items, currentHref, accumulated))
                {
                    return true;
                }

                // Not found in this subtree — remove the group crumb.
                accumulated.RemoveAt(accumulated.Count - 1);
            }
            else
            {
                // Leaf link — check if it's the current page.
                var normalizedHref = item.Href?.TrimEnd('/') ?? "";
                var normalizedCurrent = currentHref.TrimEnd('/');
                if (string.Equals(normalizedHref, normalizedCurrent, StringComparison.OrdinalIgnoreCase))
                {
                    accumulated.Add(new BreadcrumbItem(item.Label, null, true));
                    return true;
                }
            }
        }

        return false;
    }
}

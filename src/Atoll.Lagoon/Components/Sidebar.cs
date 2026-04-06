using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Navigation;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders the full sidebar navigation as an accessible <c>&lt;nav&gt;</c> element
/// with nested <c>&lt;ul&gt;</c> lists, section headings, active state highlighting,
/// and collapsible groups.
/// </summary>
/// <remarks>
/// <para>
/// Each collapsible <c>&lt;details&gt;</c> group is assigned a sequential
/// <c>data-index</c> attribute (0-based, depth-first) so that the client-side
/// sidebar state script can persist and restore open/closed state across navigations.
/// </para>
/// <para>
/// A <c>data-hash</c> attribute on the <c>&lt;nav&gt;</c> element encodes a
/// structural fingerprint of the sidebar. The client script discards persisted
/// state when the hash changes (e.g. after a config change).
/// </para>
/// <para>
/// Rendering is delegated to <c>SidebarTemplate.cshtml</c>.
/// </para>
/// </remarks>
public sealed class Sidebar : AtollComponent
{
    /// <summary>Gets or sets the resolved sidebar items to render.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<ResolvedSidebarItem> Items { get; set; } = [];

    /// <summary>Gets or sets the UI translations. Defaults to English.</summary>
    [Parameter]
    public UiTranslations Translations { get; set; } = UiTranslations.Default;

    /// <summary>
    /// Gets or sets the chevron position for collapsible group indicators.
    /// Default: <see cref="SidebarChevronPosition.End"/>.
    /// </summary>
    [Parameter]
    public SidebarChevronPosition ChevronPosition { get; set; } = SidebarChevronPosition.End;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var hash = ComputeHash(Items);
        var navLabel = System.Net.WebUtility.HtmlEncode(Translations.SidebarNavLabel);
        var counter = new GroupIndexCounter();

        var model = new SidebarModel(Items, hash, navLabel, ChevronPosition, counter);

        await ComponentRenderer.RenderSliceAsync<SidebarTemplate, SidebarModel>(
            context.Destination,
            model);
    }

    // ── Hash computation ─────────────────────────────────────────────────────

    /// <summary>
    /// Computes a DJB2 hash of all group labels in depth-first order.
    /// Used by the client to invalidate persisted state after a config change.
    /// </summary>
    private static string ComputeHash(IReadOnlyList<ResolvedSidebarItem> items)
    {
        uint hash = 5381;
        AppendHash(items, ref hash);
        return hash.ToString("x8");
    }

    private static void AppendHash(IReadOnlyList<ResolvedSidebarItem> items, ref uint hash)
    {
        foreach (var item in items)
        {
            if (!item.IsGroup) continue;
            foreach (var c in item.Label)
            {
                hash = ((hash << 5) + hash) + c;
            }
            hash = ((hash << 5) + hash) + 0; // separator
            AppendHash(item.Items, ref hash);
        }
    }
}

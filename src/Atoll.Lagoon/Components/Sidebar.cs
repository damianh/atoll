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
        WriteHtml($"<nav aria-label=\"{navLabel}\" data-hash=\"{hash}\">");
        WriteHtml(RestoreScript);
        WriteHtml("<ul>");

        var counter = new GroupIndexCounter();
        foreach (var item in Items)
        {
            if (item.IsGroup)
            {
                WriteHtml("<li class=\"sidebar-group-item\">");
                var groupFragment = ComponentRenderer.ToFragment<SidebarGroup>(
                    new Dictionary<string, object?>
                    {
                        ["Group"] = item,
                        ["ChevronPosition"] = ChevronPosition,
                        ["Counter"] = counter,
                    });
                await RenderAsync(groupFragment);
                WriteHtml("</li>");
            }
            else
            {
                var linkFragment = ComponentRenderer.ToFragment<SidebarLink>(
                    new Dictionary<string, object?> { ["Item"] = item });
                await RenderAsync(linkFragment);
            }
        }

        WriteHtml("</ul>");
        WriteHtml(ScrollRestoreScript);
        WriteHtml("</nav>");
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

    // ── Inline scripts ───────────────────────────────────────────────────────

    /// <summary>
    /// Inline script emitted BEFORE the <c>&lt;ul&gt;</c>. Runs synchronously so the
    /// <c>sl-sidebar-restore</c> custom element is defined before any
    /// <c>&lt;details&gt;</c> elements are parsed.
    /// </summary>
    private const string RestoreScript = """
        <script>
        (function(){
          try{
            if(window.matchMedia('(max-width:768px)').matches)return;
            var key='atoll:sidebar-state';
            var raw=sessionStorage.getItem(key);
            if(!raw)return;
            var state=JSON.parse(raw);
            var nav=document.querySelector('.docs-sidebar nav[data-hash]');
            if(!nav||nav.getAttribute('data-hash')!==state.hash){sessionStorage.removeItem(key);return;}
            window.__atollSidebarState=state;
          }catch(e){}
        })();
        if(!customElements.get('sl-sidebar-restore')){
          customElements.define('sl-sidebar-restore',class extends HTMLElement{
            connectedCallback(){
              try{
                var state=window.__atollSidebarState;
                if(!state||!state.open)return;
                var i=parseInt(this.dataset.index);
                if(isNaN(i)||i>=state.open.length)return;
                var val=state.open[i];
                if(val===null||val===undefined)return;
                var details=this.closest('details');
                if(!details)return;
                if(details.hasAttribute('data-active'))return;
                if(val)details.setAttribute('open','');
                else details.removeAttribute('open');
              }catch(e){}
            }
          });
        }
        </script>
        """;

    /// <summary>
    /// Inline script emitted AFTER the <c>&lt;/ul&gt;</c>. Restores sidebar scroll
    /// position from the persisted state.
    /// </summary>
    private const string ScrollRestoreScript = """
        <script>
        (function(){
          try{
            if(window.matchMedia('(max-width:768px)').matches)return;
            var state=window.__atollSidebarState;
            if(!state||typeof state.scroll!=='number')return;
            var aside=document.querySelector('.docs-sidebar');
            if(aside)aside.scrollTop=state.scroll;
          }catch(e){}
        })();
        </script>
        """;
}

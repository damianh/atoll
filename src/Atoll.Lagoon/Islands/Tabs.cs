using Atoll.Components;
using Atoll.Islands;
using Atoll.Lagoon.Components;
using Atoll.Slots;

namespace Atoll.Lagoon.Islands;

/// <summary>
/// A tabbed interface island. The server renders all tab panels (SSR/no-JS fallback).
/// Client-side JS handles tab switching and optional cross-group synchronisation via <see cref="SyncKey"/>.
/// </summary>
/// <remarks>
/// <para>
/// Supports two rendering modes:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Programmatic mode</strong>: Provide a non-empty <see cref="TabItems"/> list.
///       The server renders the full tablist header and panels with ARIA attributes.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Slot mode</strong>: Provide slot content containing <c>TabItem</c> sections
///       (e.g. from markdown authoring). The outer wrapper is rendered server-side;
///       the <c>tabs.js</c> island script progressively enhances the DOM by building
///       the tablist header from <c>data-tab-label</c> attributes.
///     </description>
///   </item>
/// </list>
/// </remarks>
[ClientLoad]
public sealed class Tabs : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-docs-tabs.js";

    /// <summary>Gets or sets an optional key for synchronising tab selections across multiple tab groups.</summary>
    [Parameter]
    public string? SyncKey { get; set; }

    /// <summary>Gets or sets the tab items to render in programmatic mode.</summary>
    [Parameter]
    public IReadOnlyList<TabItemData> TabItems { get; set; } = [];

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var syncKeyAttr = SyncKey is not null
            ? $" data-sync-key=\"{HtmlEncode(SyncKey)}\""
            : string.Empty;

        WriteHtml($"<div class=\"tabs\"{syncKeyAttr}>");

        if (TabItems.Count > 0)
        {
            await RenderProgrammaticAsync();
        }
        else if (context.Slots.HasDefaultSlot)
        {
            await context.RenderSlotAsync();
        }

        WriteHtml("</div>");
    }

    private async Task RenderProgrammaticAsync()
    {
        var prefix = Guid.NewGuid().ToString("N")[..8];

        // Tab header buttons
        WriteHtml("<div class=\"tabs-header\" role=\"tablist\">");
        for (var i = 0; i < TabItems.Count; i++)
        {
            var tab = TabItems[i];
            var tabId = $"tab-{prefix}-{i}";
            var panelId = $"panel-{prefix}-{i}";
            var isFirst = i == 0;
            var activeClass = isFirst ? " tab-button-active" : string.Empty;
            var ariaSelected = isFirst ? "true" : "false";
            var encodedLabel = HtmlEncode(tab.Label);

            WriteHtml($"<button role=\"tab\" aria-selected=\"{ariaSelected}\" aria-controls=\"{panelId}\" " +
                      $"id=\"{tabId}\" class=\"tab-button{activeClass}\" data-tab-label=\"{encodedLabel}\">");

            if (tab.IconName is { } iconName)
            {
                var iconProps = new Dictionary<string, object?> { ["Name"] = iconName };
                var iconFragment = ComponentRenderer.ToFragment<Icon>(iconProps);
                await RenderAsync(iconFragment);
            }

            WriteText(tab.Label);
            WriteHtml("</button>");
        }
        WriteHtml("</div>");

        // Tab panels
        for (var i = 0; i < TabItems.Count; i++)
        {
            var tab = TabItems[i];
            var tabId = $"tab-{prefix}-{i}";
            var panelId = $"panel-{prefix}-{i}";
            var hiddenAttr = i == 0 ? string.Empty : " hidden";

            WriteHtml($"<div role=\"tabpanel\" id=\"{panelId}\" aria-labelledby=\"{tabId}\" class=\"tab-panel\"{hiddenAttr}>");
            await RenderAsync(tab.Content);
            WriteHtml("</div>");
        }
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}


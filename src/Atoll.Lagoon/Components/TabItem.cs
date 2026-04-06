using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a single tab panel for use inside a <see cref="Atoll.Lagoon.Islands.Tabs"/> component.
/// Each instance renders a <c>&lt;section&gt;</c> element with a <c>data-tab-label</c> attribute
/// that the <c>tabs.js</c> island script uses to build the tablist header and manage visibility.
/// </summary>
/// <remarks>
/// <para>
/// This component is designed for slot-based (markdown-authored) usage:
/// <code>
/// &lt;Tabs SyncKey="pkg"&gt;
///   &lt;TabItem Label="npm"&gt;content&lt;/TabItem&gt;
/// &lt;/Tabs&gt;
/// </code>
/// </para>
/// <para>
/// All panels are rendered visible by default (progressive enhancement / no-JS fallback).
/// The <c>tabs.js</c> script hides non-active panels after DOM construction.
/// </para>
/// </remarks>
public sealed class TabItem : AtollComponent
{
    /// <summary>Gets or sets the display label shown in the tab button.</summary>
    [Parameter(Required = true)]
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional icon to display in the tab button.</summary>
    [Parameter]
    public IconName? IconName { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var encodedLabel = HtmlEncode(Label);
        var iconAttr = IconName is { } icon
            ? $" data-tab-icon=\"{HtmlEncode(icon.ToString())}\""
            : string.Empty;

        WriteHtml($"<section class=\"tab-panel\" data-tab-label=\"{encodedLabel}\"{iconAttr}>");
        await RenderSlotAsync();
        WriteHtml("</section>");
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}

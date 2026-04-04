using Atoll.Components;
using Atoll.Slots;
using SplashAddonLayout = Atoll.Lagoon.Layouts.SplashLayout;

namespace Docs.Layouts;

/// <summary>
/// Site-specific wrapper layout for splash/landing pages. Wires
/// <see cref="DocsSetup.Config"/> into the <c>Atoll.Lagoon</c> addon
/// <see cref="SplashAddonLayout"/>, providing a full-width, sidebar-free
/// page with the shared header and footer.
/// </summary>
public sealed class SplashSiteLayout : AtollComponent
{
    /// <summary>Gets or sets the page title shown in the &lt;title&gt; tag.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets an optional page description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var config = DocsSetup.Config;

        // Pass the default slot (page content) through to the addon layout
        var pageSlot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);

        var addonProps = new Dictionary<string, object?>
        {
            ["Config"] = config,
            ["PageTitle"] = PageTitle,
            ["PageDescription"] = PageDescription,
        };

        var addonSlots = SlotCollection.FromDefault(pageSlot);
        var addonFragment = ComponentRenderer.ToFragment<SplashAddonLayout>(addonProps, addonSlots);
        await RenderAsync(addonFragment);
    }
}

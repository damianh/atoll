using Atoll.Components;
using Atoll.Lagoon.Assets;
using Atoll.Lagoon.Configuration;
using Atoll.Slots;

namespace Atoll.Lagoon.Layouts;

/// <summary>
/// A full-width, sidebar-free layout for splash and landing pages. Assembles the
/// full HTML document shell with the shared header (logo, search, theme toggle) and
/// footer, but omits the sidebar, table of contents, breadcrumbs, pagination, and
/// mobile-nav toggle — yielding a wide, uncluttered canvas suitable for landing pages.
/// </summary>
/// <remarks>
/// Usage: set <see cref="Config"/> (required), optional page-specific parameters
/// (<see cref="PageTitle"/>, <see cref="PageDescription"/>), then place page content
/// (e.g. a <c>Hero</c> component) in the default slot.
/// Rendering is delegated to <c>SplashLayoutTemplate.cshtml</c>.
/// </remarks>
public sealed class SplashLayout : AtollComponent
{
    /// <summary>Gets or sets the docs site configuration. Required.</summary>
    [Parameter(Required = true)]
    public DocsConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the page-specific title. Appended to the site title in the &lt;title&gt; tag.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page-specific description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var logoSrc = !string.IsNullOrEmpty(Config.LogoSrc)
            ? Config.LogoSrc
            : LagoonAssets.DefaultFaviconPath;

        var model = new SplashLayoutModel(Config, PageTitle, PageDescription, logoSrc, Config.EnableMermaid);

        // Pass the page content slot through to the Razor template.
        var pageSlot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);
        var templateSlots = SlotCollection.FromDefault(pageSlot);

        await ComponentRenderer.RenderSliceAsync<SplashLayoutTemplate, SplashLayoutModel>(
            context.Destination,
            model,
            templateSlots);
    }
}

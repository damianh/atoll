using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Routing;
using Atoll.Lagoon.Components;
using Docs.Layouts;

namespace Docs.Pages;

/// <summary>
/// The documentation site landing page. Uses <see cref="SplashSiteLayout"/> for a
/// full-width, sidebar-free layout with a hero section, tagline, and CTA buttons.
/// </summary>
[Layout(typeof(SplashSiteLayout))]
[PageRoute("/")]
public sealed class IndexPage : AtollComponent, IAtollPage
{
    /// <summary>
    /// Gets or sets the collection query (required by convention; not used directly on this page).
    /// </summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var actions = new List<HeroAction>
        {
            new("Get Started", "/docs/getting-started", HeroActionVariant.Primary),
            new("View on GitHub", "https://github.com/damianh/atoll", HeroActionVariant.Secondary),
        };

        var heroFragment = ComponentRenderer.ToFragment<Hero>(new Dictionary<string, object?>
        {
            ["Title"] = "Atoll",
            ["Tagline"] = "A .NET-native static-site framework inspired by Astro. Build fast, content-rich sites with pure C# — no JavaScript required.",
            ["Actions"] = (IReadOnlyList<HeroAction>)actions,
        });

        await RenderAsync(heroFragment);
    }
}

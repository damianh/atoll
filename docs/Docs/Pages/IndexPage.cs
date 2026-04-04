using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Routing;
using Atoll.Lagoon.Components;
using Docs.Layouts;

namespace Docs.Pages;

/// <summary>
/// The documentation site landing page. Displays a hero section with a tagline
/// and feature highlights, linking visitors to the Getting Started guide.
/// </summary>
[Layout(typeof(SiteLayout))]
[PageRoute("/")]
public sealed class IndexPage : AtollComponent, IAtollPage
{
    /// <summary>
    /// Gets or sets the collection query used by the layout's sidebar navigation.
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

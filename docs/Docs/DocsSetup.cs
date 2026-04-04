using Atoll.Lagoon.Configuration;

namespace Docs;

/// <summary>
/// Provides the <see cref="DocsConfig"/> for the Atoll documentation site.
/// Used by <see cref="Layouts.SiteLayout"/> to configure the <c>Atoll.Lagoon</c> addon layout.
/// </summary>
public static class DocsSetup
{
    /// <summary>
    /// Gets the documentation site configuration, including title, sidebar items, and feature flags.
    /// </summary>
    public static DocsConfig Config { get; } = new DocsConfig
    {
        Title = "Atoll",
        Description = "A .NET-native static-site framework inspired by Astro.",
        BasePath = "",
        EnableMermaid = false,
        Social =
        [
            new SocialLink("GitHub", "https://github.com/damianh/atoll", SocialIcon.GitHub),
        ],
        Sidebar =
        [
            new SidebarItem
            {
                Label = "Getting Started",
                Link = "/docs/getting-started",
            },
            new SidebarItem
            {
                Label = "Basics",
                Items =
                [
                    new SidebarItem { Label = "Components",              Link = "/docs/components" },
                    new SidebarItem { Label = "Layouts",                 Link = "/docs/layouts" },
                    new SidebarItem { Label = "Pages & Routing",         Link = "/docs/pages-and-routing" },
                    new SidebarItem { Label = "Configuration",           Link = "/docs/configuration" },
                ],
            },
            new SidebarItem
            {
                Label = "Features",
                Items =
                [
                    new SidebarItem { Label = "Content Collections",     Link = "/docs/content-collections" },
                    new SidebarItem { Label = "CSS Scoping",             Link = "/docs/css-scoping" },
                    new SidebarItem { Label = "Islands Architecture",    Link = "/docs/islands" },
                    new SidebarItem { Label = "Static Site Generation",  Link = "/docs/static-site-generation" },
                ],
            },
            new SidebarItem
            {
                Label = "Reference",
                Items =
                [
                    new SidebarItem { Label = "API Endpoints",           Link = "/docs/api-endpoints" },
                ],
            },
            new SidebarItem
            {
                Label = "Lagoon Theme",
                Items =
                [
                    new SidebarItem { Label = "Overview",              Link = "/docs/lagoon/overview" },
                    new SidebarItem { Label = "Configuration",         Link = "/docs/lagoon/configuration" },
                    new SidebarItem { Label = "Sidebar Navigation",    Link = "/docs/lagoon/sidebar" },
                    new SidebarItem { Label = "Theme & Styling",       Link = "/docs/lagoon/theming" },
                    new SidebarItem { Label = "Components & Layout",   Link = "/docs/lagoon/components" },
                    new SidebarItem { Label = "Site Search",           Link = "/docs/lagoon/search" },
                    new SidebarItem { Label = "Islands & Mermaid",     Link = "/docs/lagoon/islands-and-mermaid" },
                    new SidebarItem { Label = "Starlight Comparison",  Link = "/docs/lagoon/starlight-comparison" },
                ],
            },
        ],
    };
}

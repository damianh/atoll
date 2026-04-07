using Atoll.Build.Content.Collections;
using Atoll.Charts.Islands;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.Redirects;
using Atoll.Mermaid.Islands;

namespace Docs;

/// <summary>
/// Provides the <see cref="DocsConfig"/> for the Atoll documentation site.
/// Used by <see cref="Layouts.SiteLayout"/> to configure the <c>Atoll.Lagoon</c> addon layout.
/// </summary>
public static class DocsSetup
{
    // Ensure the Atoll.Mermaid assembly is referenced so the dev server's
    // island asset discovery (which only scans direct assembly references)
    // can find MermaidIslandAssetProvider.
    private static readonly Type MermaidAssetProviderType = typeof(MermaidIslandAssetProvider);

    // Ensure the Atoll.Charts assembly is referenced so the dev server's
    // island asset discovery can find ChartIslandAssetProvider.
    private static readonly Type ChartAssetProviderType = typeof(ChartIslandAssetProvider);
    /// <summary>
    /// Gets the documentation site configuration, including title, sidebar items, and feature flags.
    /// </summary>
    public static DocsConfig Config { get; } = new DocsConfig
    {
        Title = "Atoll",
        Description = "A .NET-native static-site framework inspired by Astro.",
        BasePath = "/atoll",
        EnableMermaid = true,
        EnableSyntaxHighlighting = true,
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
                    new SidebarItem { Label = "MDA Format",              Link = "/docs/mda-format" },
                    new SidebarItem { Label = "Content Collections",     Link = "/docs/content-collections" },
                    new SidebarItem { Label = "CSS Scoping",             Link = "/docs/css-scoping" },
                    new SidebarItem { Label = "Islands Architecture",    Link = "/docs/islands" },
                    new SidebarItem { Label = "Static Site Generation",  Link = "/docs/static-site-generation" },
                    new SidebarItem { Label = "HTTP Caching",            Link = "/docs/caching" },
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
                Label = "Lagoon Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",              Link = "/docs/lagoon/overview" },
                    new SidebarItem { Label = "Configuration",         Link = "/docs/lagoon/configuration" },
                    new SidebarItem { Label = "Sidebar Navigation",    Link = "/docs/lagoon/sidebar" },
                    new SidebarItem { Label = "Theme & Styling",       Link = "/docs/lagoon/theming" },
                    new SidebarItem { Label = "Components & Layout",   Link = "/docs/lagoon/components" },
                    new SidebarItem { Label = "Site Search",           Link = "/docs/lagoon/search" },
                    new SidebarItem { Label = "Global Banner",         Link = "/docs/lagoon/banner" },
                    new SidebarItem { Label = "LLM Content Export",   Link = "/docs/lagoon/llms-txt" },
                    new SidebarItem { Label = "Custom 404 Pages",      Link = "/docs/lagoon/custom-404" },
                    new SidebarItem { Label = "Islands & Mermaid",     Link = "/docs/lagoon/islands-and-mermaid" },
                    new SidebarItem { Label = "Content Components",   Link = "/docs/lagoon/content-components" },
                    new SidebarItem { Label = "Internationalisation",  Link = "/docs/lagoon/i18n" },
                    new SidebarItem { Label = "Versioning",            Link = "/docs/lagoon/versioning" },
                    new SidebarItem { Label = "Starlight Comparison",  Link = "/docs/lagoon/starlight-comparison" },
                ],
            },
            new SidebarItem
            {
                Label = "Reef Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",             Link = "/docs/reef/overview" },
                    new SidebarItem { Label = "Configuration",        Link = "/docs/reef/configuration" },
                    new SidebarItem { Label = "Components",           Link = "/docs/reef/components" },
                    new SidebarItem { Label = "Islands",              Link = "/docs/reef/islands" },
                    new SidebarItem { Label = "Views & Layouts",      Link = "/docs/reef/views-and-layouts" },
                    new SidebarItem { Label = "Feeds & SEO",          Link = "/docs/reef/feeds-and-seo" },
                    new SidebarItem { Label = "Series & Navigation",  Link = "/docs/reef/series-and-navigation" },
                    new SidebarItem { Label = "Live Demo",             Link = "/docs/reef/live-demo" },
                ],
            },
            new SidebarItem
            {
                Label = "Charts Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/docs/charts/overview" },
                ],
            },
            new SidebarItem
            {
                Label = "Mermaid Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/docs/mermaid/overview" },
                ],
            },
            new SidebarItem
            {
                Label = "DrawIO Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/docs/drawio/overview" },
                ],
            },
            new SidebarItem
            {
                Label = "Giscus Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/docs/giscus/overview" },
                ],
            },
            new SidebarItem
            {
                Label = "Annotations Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/docs/annotations/overview" },
                ],
            },
        ],
    };

    /// <summary>
    /// Extracts redirect sources from a collection of documentation content entries.
    /// Each entry's canonical URL and <c>redirectFrom</c> frontmatter field are combined
    /// into a <see cref="RedirectSource"/> for use with <see cref="RedirectCollector"/>.
    /// </summary>
    /// <param name="entries">The loaded documentation content entries.</param>
    /// <returns>
    /// A list of <see cref="RedirectSource"/> instances, one per entry that declares
    /// redirect sources (entries with no <c>redirectFrom</c> are included with an empty list).
    /// </returns>
    /// <example>
    /// <code>
    /// var entries = await query.GetEntriesAsync&lt;DocSchema&gt;("docs");
    /// var sources = DocsSetup.BuildRedirectSources(entries);
    /// var collector = new RedirectCollector(DocsSetup.Config.Redirects);
    /// var redirectMap = collector.Collect(sources);
    /// </code>
    /// </example>
    public static IReadOnlyList<RedirectSource> BuildRedirectSources(
        IEnumerable<ContentEntry<DocSchema>> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var sources = new List<RedirectSource>();
        foreach (var entry in entries)
        {
            sources.Add(new RedirectSource(
                "/" + entry.Slug,
                entry.Data.RedirectFrom ?? []));
        }

        return sources;
    }
}

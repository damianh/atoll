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
                Link = "/getting-started",
            },
            new SidebarItem
            {
                Label = "Basics",
                Items =
                [
                    new SidebarItem { Label = "Components",              Link = "/components" },
                    new SidebarItem { Label = "Layouts",                 Link = "/layouts" },
                    new SidebarItem { Label = "Pages & Routing",         Link = "/pages-and-routing" },
                    new SidebarItem { Label = "Configuration",           Link = "/configuration" },
                ],
            },
            new SidebarItem
            {
                Label = "Features",
                Items =
                [
                    new SidebarItem { Label = "MDA Format",              Link = "/mda-format" },
                    new SidebarItem { Label = "Content Collections",     Link = "/content-collections" },
                    new SidebarItem { Label = "CSS Scoping",             Link = "/css-scoping" },
                    new SidebarItem { Label = "Islands Architecture",    Link = "/islands" },
                    new SidebarItem { Label = "Static Site Generation",  Link = "/static-site-generation" },
                    new SidebarItem { Label = "HTTP Caching",            Link = "/caching" },
                ],
            },
            new SidebarItem
            {
                Label = "Reference",
                Items =
                [
                    new SidebarItem { Label = "API Endpoints",           Link = "/api-endpoints" },
                ],
            },
            new SidebarItem
            {
                Label = "Lagoon Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",              Link = "/lagoon/overview" },
                    new SidebarItem { Label = "Configuration",         Link = "/lagoon/configuration" },
                    new SidebarItem { Label = "Sidebar Navigation",    Link = "/lagoon/sidebar" },
                    new SidebarItem { Label = "Theme & Styling",       Link = "/lagoon/theming" },
                    new SidebarItem { Label = "Components & Layout",   Link = "/lagoon/components" },
                    new SidebarItem { Label = "Site Search",           Link = "/lagoon/search" },
                    new SidebarItem { Label = "Global Banner",         Link = "/lagoon/banner" },
                    new SidebarItem { Label = "LLM Content Export",   Link = "/lagoon/llms-txt" },
                    new SidebarItem { Label = "Custom 404 Pages",      Link = "/lagoon/custom-404" },
                    new SidebarItem { Label = "Islands, Mermaid & Charts", Link = "/lagoon/islands-and-mermaid" },
                    new SidebarItem { Label = "Content Components",   Link = "/lagoon/content-components" },
                    new SidebarItem { Label = "Internationalisation",  Link = "/lagoon/i18n" },
                    new SidebarItem { Label = "Versioning",            Link = "/lagoon/versioning" },
                    new SidebarItem { Label = "Starlight Comparison",  Link = "/lagoon/starlight-comparison" },
                ],
            },
            new SidebarItem
            {
                Label = "Reef Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",             Link = "/reef/overview" },
                    new SidebarItem { Label = "Configuration",        Link = "/reef/configuration" },
                    new SidebarItem { Label = "Components",           Link = "/reef/components" },
                    new SidebarItem { Label = "Islands",              Link = "/reef/islands" },
                    new SidebarItem { Label = "Views & Layouts",      Link = "/reef/views-and-layouts" },
                    new SidebarItem { Label = "Feeds & SEO",          Link = "/reef/feeds-and-seo" },
                    new SidebarItem { Label = "Series & Navigation",  Link = "/reef/series-and-navigation" },
                    new SidebarItem { Label = "Live Demo",             Link = "/reef/live-demo" },
                ],
            },
            new SidebarItem
            {
                Label = "Charts Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/charts/overview" },
                ],
            },
            new SidebarItem
            {
                Label = "Mermaid Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/mermaid/overview" },
                ],
            },
            new SidebarItem
            {
                Label = "DrawIO Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/drawio/overview" },
                ],
            },
            new SidebarItem
            {
                Label = "Giscus Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/giscus/overview" },
                ],
            },
            new SidebarItem
            {
                Label = "Annotations Plugin",
                Items =
                [
                    new SidebarItem { Label = "Overview",  Link = "/annotations/overview" },
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

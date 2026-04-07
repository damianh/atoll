using Atoll.Annotations;
using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Lagoon.Navigation;
using Atoll.Slots;
using Docs.Pages;
using AddonLayout = Atoll.Lagoon.Layouts.DocsLayout;

namespace Docs.Layouts;

/// <summary>
/// Site-specific wrapper layout for the Atoll documentation site.
/// Wires <see cref="DocsSetup.Config"/> into the <c>Atoll.Lagoon</c> addon
/// <see cref="AddonLayout"/>, building sidebar, pagination, and breadcrumbs
/// from the content collection query and current page slug.
/// </summary>
public sealed class SiteLayout : AtollComponent
{
    /// <summary>Gets or sets the collection query for sidebar and navigation building.</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <summary>Gets or sets the current page slug used to determine active nav items.</summary>
    [Parameter]
    public string Slug { get; set; } = "";

    /// <summary>Gets or sets the page title shown in the &lt;title&gt; tag and breadcrumbs.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets an optional page description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var config = DocsSetup.Config;
        var currentHref = string.IsNullOrEmpty(Slug) ? "/" : $"/docs/{Slug}";

        // Load the current page entry for per-page metadata (head injection, etc.)
        var currentEntry = !string.IsNullOrEmpty(Slug)
            ? Query.GetEntry<DocSchema>("docs", Slug)
            : null;

        // Extract headings from the markdown entry for the table of contents.
        // We must resolve these here because the page renders as a slot inside
        // DocsLayout, and the layout needs headings before the ToC renders.
        var headings = currentEntry is not null
            ? Query.Render(currentEntry).Headings
            : (IReadOnlyList<MarkdownHeading>)[];

        // Build sidebar entries from the docs collection, excluding the reserved 404 page
        var entries = Query.GetCollection<DocSchema>("docs")
            .Where(e => e.Slug != DocsPage.NotFoundSlug)
            .Select(e => new SidebarEntry(e.Data.Title, $"/docs/{e.Slug}", e.Slug, e.Data.Order, null))
            .ToList();

        // Resolve sidebar tree for current page
        var builder = new SidebarBuilder(config.Sidebar, entries);
        var sidebarItems = builder.Build(currentHref);

        // Resolve pagination
        var paginationResolver = new PaginationResolver(sidebarItems, flatten: true);
        var pagination = paginationResolver.Resolve(currentHref);

        // Build breadcrumbs
        var breadcrumbBuilder = new BreadcrumbBuilder(sidebarItems);
        var breadcrumbs = breadcrumbBuilder.Build(currentHref);

        // Pass the default slot (page content) through to the addon layout.
        var pageSlot = context.Slots.GetSlotFragment(SlotCollection.DefaultSlotName);

        // Render via addon layout
        var addonProps = new Dictionary<string, object?>
        {
            ["Config"] = config,
            ["PageTitle"] = currentEntry?.Data.Title ?? PageTitle,
            ["PageDescription"] = currentEntry?.Data.Description ?? PageDescription,
            ["Headings"] = headings,
            ["SidebarItems"] = sidebarItems,
            ["Previous"] = pagination.Previous,
            ["Next"] = pagination.Next,
            ["BreadcrumbItems"] = breadcrumbs,
            ["PageHeadContent"] = currentEntry?.Data.Head,
        };

        var addonSlots = SlotCollection.FromDefault(pageSlot);
        var addonFragment = ComponentRenderer.ToFragment<AddonLayout>(addonProps, addonSlots);
        await RenderAsync(addonFragment);

        // Render the text-annotation island so readers can select text and
        // submit contextual feedback as a GitHub Discussion.
        var annotationProps = new Dictionary<string, object?>
        {
            [nameof(TextAnnotation.Repo)] = "damianh/atoll",
            [nameof(TextAnnotation.Target)] = AnnotationTarget.Discussion,
            [nameof(TextAnnotation.Category)] = "General",
        };
        var annotationFragment = ComponentRenderer.ToFragment<TextAnnotation>(annotationProps);
        await RenderAsync(annotationFragment);
    }
}

using Atoll.Build.Content.Collections;
using Atoll.Components;
using Atoll.Islands;
using Atoll.Reef.Components;
using Atoll.Reef.Configuration;
using Atoll.Reef.Islands;
using Atoll.Reef.Navigation;
using Atoll.Routing;
using Atoll.Slots;

namespace Atoll.Samples.Articles.Pages;

/// <summary>
/// The articles listing page. Displays all published articles sorted by date
/// using <see cref="ArticleList"/> from Atoll.Reef. Includes the
/// <see cref="ArticleFilter"/> and <see cref="ViewToggle"/> islands for
/// client-side filtering and view switching.
/// </summary>
[Layout(typeof(ArticlesLayout))]
[PageRoute("/articles")]
public sealed class ArticleIndexPage : AtollComponent, IAtollPage
{
    /// <summary>Gets or sets the collection query for loading articles.</summary>
    [Parameter(Required = true)]
    public CollectionQuery Query { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<h1 style=\"margin-bottom:1.5rem;\">Articles</h1>");

        var entries = Query.GetCollection<ArticleSchema>("articles",
            entry => !entry.Data.Draft);

        var items = ArticleListItemBuilder.Build(
            entries.OrderByDescending(e => e.Data.PubDate).ToList());

        if (items.Count == 0)
        {
            WriteHtml("<p>No articles yet. Check back soon!</p>");
            return;
        }

        // Collect unique tags and authors for the filter island
        var tags = items
            .SelectMany(i => i.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        var authors = items
            .Where(i => i.Author is not null)
            .Select(i => i.Author!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        // Controls bar: view toggle + article filter
        WriteHtml("<div class=\"reef-listing\" data-view-container>");
        WriteHtml("<div class=\"reef-listing-controls\">");

        await IslandRenderer.RenderIslandAsync<ViewToggle>(
            context.Destination,
            new ViewToggle().CreateMetadata()!,
            new Dictionary<string, object?>
            {
                [nameof(ViewToggle.CurrentView)] = ArticlesConfig.Current.DefaultView,
            },
            SlotCollection.Empty);

        await IslandRenderer.RenderIslandAsync<ArticleFilter>(
            context.Destination,
            new ArticleFilter().CreateMetadata()!,
            new Dictionary<string, object?>
            {
                [nameof(ArticleFilter.Tags)] = (IReadOnlyList<string>)tags,
                [nameof(ArticleFilter.Authors)] = (IReadOnlyList<string>)authors,
            },
            SlotCollection.Empty);

        WriteHtml("</div>");

        var listFragment = ComponentRenderer.ToFragment<ArticleList>(new Dictionary<string, object?>
        {
            [nameof(ArticleList.Items)] = (IReadOnlyList<ArticleListItem>)items,
            [nameof(ArticleList.BasePath)] = ArticlesConfig.Current.BasePath,
        });
        await RenderAsync(listFragment);
        WriteHtml("</div>"); // close data-view-container
    }
}

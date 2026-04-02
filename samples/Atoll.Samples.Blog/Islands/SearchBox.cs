using Atoll.Core.Components;
using Atoll.Core.Islands;

namespace Atoll.Samples.Blog.Islands;

/// <summary>
/// A search island component that renders a search input for filtering blog posts.
/// Uses <c>client:load</c> for immediate hydration so the search is interactive on load.
/// </summary>
[ClientLoad]
public sealed class SearchBox : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/search.js";

    /// <summary>
    /// Gets or sets the placeholder text for the search input.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Search posts...";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"search-box\" style=\"margin-bottom: 1.5rem;\">");
        WriteHtml("<input type=\"search\" id=\"search-input\" placeholder=\"");
        WriteText(Placeholder);
        WriteHtml("\" style=\"width: 100%; padding: 0.5rem 0.75rem; border: 1px solid var(--color-border); border-radius: 0.25rem; font-size: 1rem;\" />");
        WriteHtml("<div id=\"search-results\"></div>");
        WriteHtml("</div>");
        return Task.CompletedTask;
    }
}

using Atoll.Components;
using Atoll.Lagoon.Islands;
using Atoll.Instructions;
using Atoll.Rendering;

namespace Atoll.Lagoon.Tests.Islands;

public sealed class SearchDialogTests
{
    [Fact]
    public async Task ShouldRenderSearchTriggerButton()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("id=\"search-trigger\"");
        html.ShouldContain("<button");
    }

    [Fact]
    public async Task ShouldRenderKeyboardShortcutHint()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("Ctrl+K");
    }

    [Fact]
    public async Task ShouldRenderDialogElement()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("<dialog");
        html.ShouldContain("id=\"search-dialog\"");
    }

    [Fact]
    public async Task ShouldRenderSearchInput()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("id=\"search-input\"");
        html.ShouldContain("type=\"search\"");
    }

    [Fact]
    public async Task ShouldRenderSearchResults()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("id=\"search-results\"");
    }

    [Fact]
    public async Task ShouldRenderDefaultPlaceholder()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("Search docs...");
    }

    [Fact]
    public async Task ShouldRenderCustomPlaceholder()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Placeholder"] = "Find articles..." };
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("Find articles...");
    }

    [Fact]
    public void ShouldHaveClientIdleDirective()
    {
        var island = new SearchDialog();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
    }

    [Fact]
    public void ShouldHaveCorrectClientModuleUrl()
    {
        var island = new SearchDialog();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-docs-search-dialog.js");
    }

    [Fact]
    public async Task ShouldRenderAsIslandWrapper()
    {
        var dest = new StringRenderDestination();
        var island = new SearchDialog();
        await island.RenderIslandAsync(dest);
        var html = dest.GetOutput();

        html.ShouldContain("<atoll-island");
        html.ShouldContain("client=\"idle\"");
        html.ShouldContain("component-url=\"/scripts/atoll-docs-search-dialog.js\"");
    }

    [Fact]
    public async Task ShouldRenderTriggerWithAriaHasPopup()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("aria-haspopup=\"dialog\"");
    }

    [Fact]
    public async Task ShouldRenderCloseButton()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("id=\"search-close\"");
        html.ShouldContain("aria-label=\"Close search\"");
    }

    [Fact]
    public async Task ShouldRenderIslandWrapperViaComponentRenderer()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        // ComponentRenderer automatically wraps island components in <atoll-island>
        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
        html.ShouldContain("client=\"idle\"");
        html.ShouldContain("component-url=\"/scripts/atoll-docs-search-dialog.js\"");
    }

    [Fact]
    public async Task ShouldRenderBasePathDataAttribute()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["BasePath"] = "/atoll" };
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("data-base-path=\"/atoll\"");
    }

    [Fact]
    public async Task ShouldRenderEmptyBasePathByDefault()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("data-base-path=\"\"");
    }

    [Fact]
    public async Task ShouldTrimTrailingSlashFromBasePath()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["BasePath"] = "/atoll/" };
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, props);
        var html = dest.GetOutput();

        html.ShouldContain("data-base-path=\"/atoll\"");
    }

    [Fact]
    public async Task ShouldRenderTopicFilterContainer()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("id=\"search-topics\"");
        html.ShouldContain("class=\"search-topic-filter\"");
    }

    [Fact]
    public async Task ShouldRenderTopicFilterLabelDataAttribute()
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<SearchDialog>(dest, new Dictionary<string, object?>());
        var html = dest.GetOutput();

        html.ShouldContain("data-topic-filter-label=\"Filter by topic\"");
    }
}

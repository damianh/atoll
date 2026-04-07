using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Lagoon.Islands;
using Atoll.Rendering;

namespace Atoll.Lagoon.Tests.Islands;

/// <summary>
/// End-to-end integration tests proving the full markdown → HTML pipeline for
/// <c>&lt;Tabs&gt;</c> / <c>&lt;TabItem&gt;</c> syntax, from raw markdown string
/// through <see cref="MarkdownRenderer"/> to rendered HTML output.
/// </summary>
public sealed class TabsMarkdownIntegrationTests
{
    private static readonly MarkdownOptions TabsMarkdownOptions = new()
    {
        Components = new ComponentMap()
            .Add<Tabs>("tabs")
            .Add<TabItem>("tab-item"),
    };

    private static async Task<string> RenderMarkdownAsync(string markdown)
    {
        var result = MarkdownRenderer.Render(markdown, TabsMarkdownOptions);

        if (result.Fragments is null)
        {
            return result.Html;
        }

        var rendered = new RenderedContent(
            result.Html,
            result.Headings,
            result.Fragments,
            result.AllReferences);
        var component = ContentComponent.FromRenderedContent(rendered);

        var dest = new StringRenderDestination();
        await component.RenderAsync(new RenderContext(dest));
        return dest.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderTabsWrapperWithSyncKey()
    {
        var markdown = """
            <Tabs SyncKey="pkg">
            <TabItem Label="npm">Install with npm.</TabItem>
            </Tabs>
            """;

        var html = await RenderMarkdownAsync(markdown);

        html.ShouldContain("class=\"tabs\"");
        html.ShouldContain("data-sync-key=\"pkg\"");
    }

    [Fact]
    public async Task ShouldRenderTabItemSectionsWithDataTabLabel()
    {
        var markdown = """
            <Tabs SyncKey="pkg">
            <TabItem Label="npm">npm content</TabItem>
            <TabItem Label="yarn">yarn content</TabItem>
            </Tabs>
            """;

        var html = await RenderMarkdownAsync(markdown);

        html.ShouldContain("data-tab-label=\"npm\"");
        html.ShouldContain("data-tab-label=\"yarn\"");
    }

    [Fact]
    public async Task ShouldRenderInnerMarkdownContentInsideTabItems()
    {
        var markdown = """
            <Tabs SyncKey="pkg">
            <TabItem Label="npm">

            Install with `npm install`.

            </TabItem>
            </Tabs>
            """;

        var html = await RenderMarkdownAsync(markdown);

        // Inline code should be rendered as <code> by Markdig.
        html.ShouldContain("<code>");
        html.ShouldContain("npm install");
    }

    [Fact]
    public async Task ShouldRenderAtollIslandWrapperForTabs()
    {
        var markdown = """
            <Tabs SyncKey="pkg">
            <TabItem Label="npm">content</TabItem>
            </Tabs>
            """;

        var html = await RenderMarkdownAsync(markdown);

        // Tabs is a VanillaJsIsland — it wraps in an atoll-island web component.
        html.ShouldContain("atoll-island");
    }

    [Fact]
    public async Task ShouldNotContainRawPlaceholderComments()
    {
        var markdown = """
            <Tabs SyncKey="pkg">
            <TabItem Label="npm">content</TabItem>
            <TabItem Label="yarn">content</TabItem>
            </Tabs>
            """;

        var html = await RenderMarkdownAsync(markdown);

        // No unresolved component placeholder comments should appear in output.
        html.ShouldNotContain("<!--atoll");
    }

    [Fact]
    public async Task ShouldRenderMultipleTabItemsAsTabPanelSections()
    {
        var markdown = """
            <Tabs SyncKey="pkg">
            <TabItem Label="npm">npm content here</TabItem>
            <TabItem Label="yarn">yarn content here</TabItem>
            <TabItem Label="pnpm">pnpm content here</TabItem>
            </Tabs>
            """;

        var html = await RenderMarkdownAsync(markdown);

        html.ShouldContain("data-tab-label=\"npm\"");
        html.ShouldContain("data-tab-label=\"yarn\"");
        html.ShouldContain("data-tab-label=\"pnpm\"");
        html.ShouldContain("npm content here");
        html.ShouldContain("yarn content here");
        html.ShouldContain("pnpm content here");
    }
}

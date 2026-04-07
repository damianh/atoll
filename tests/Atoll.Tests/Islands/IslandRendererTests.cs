using Atoll.Components;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Tests.Islands;

public sealed class IslandRendererTests
{
    // ── GenerateOpeningTag tests ──

    [Fact]
    public void GenerateOpeningTagShouldIncludeComponentUrl()
    {
        var metadata = new IslandMetadata("/components/Counter.js", ClientDirectiveType.Load);

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldContain("component-url=\"/components/Counter.js\"");
    }

    [Fact]
    public void GenerateOpeningTagShouldIncludeComponentExport()
    {
        var metadata = new IslandMetadata("/components/Counter.js", ClientDirectiveType.Load);

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldContain("component-export=\"default\"");
    }

    [Fact]
    public void GenerateOpeningTagShouldIncludeCustomExport()
    {
        var metadata = new IslandMetadata("/components/Counter.js", ClientDirectiveType.Load)
        {
            ComponentExport = "Counter"
        };

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldContain("component-export=\"Counter\"");
    }

    [Theory]
    [InlineData(ClientDirectiveType.Load, "load")]
    [InlineData(ClientDirectiveType.Idle, "idle")]
    [InlineData(ClientDirectiveType.Visible, "visible")]
    [InlineData(ClientDirectiveType.Media, "media")]
    public void GenerateOpeningTagShouldIncludeDirectiveType(
        ClientDirectiveType directive,
        string expectedValue)
    {
        var metadata = new IslandMetadata("/c.js", directive);

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldContain($"client=\"{expectedValue}\"");
    }

    [Fact]
    public void GenerateOpeningTagShouldIncludeSsrAttribute()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldContain(" ssr");
    }

    [Fact]
    public void GenerateOpeningTagShouldIncludeSerializedProps()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);
        var serializedProps = "{\"count\":[0,42]}";

        var tag = IslandRenderer.GenerateOpeningTag(metadata, serializedProps);

        // Props should be HTML-encoded in the attribute
        tag.ShouldContain("props=\"");
    }

    [Fact]
    public void GenerateOpeningTagShouldIncludeOptsWithDisplayName()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load)
        {
            DisplayName = "Counter"
        };

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldContain("opts=\"");
        // The opts JSON should contain the display name
        tag.ShouldContain("Counter");
    }

    [Fact]
    public void GenerateOpeningTagShouldIncludeDirectiveValueInOpts()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Media)
        {
            DirectiveValue = "(max-width: 768px)"
        };

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldContain("(max-width: 768px)");
    }

    [Fact]
    public void GenerateOpeningTagShouldIncludeBeforeHydrationUrl()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load)
        {
            BeforeHydrationUrl = "/scripts/before.js"
        };

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldContain("before-hydration-url=\"/scripts/before.js\"");
    }

    [Fact]
    public void GenerateOpeningTagShouldOmitBeforeHydrationUrlWhenNull()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldNotContain("before-hydration-url");
    }

    [Fact]
    public void GenerateOpeningTagShouldStartWithAtollIslandTag()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldStartWith("<atoll-island");
        tag.ShouldEndWith(">");
    }

    [Fact]
    public void GenerateOpeningTagShouldEscapeHtmlInComponentUrl()
    {
        var metadata = new IslandMetadata("/c.js?a=1&b=2", ClientDirectiveType.Load);

        var tag = IslandRenderer.GenerateOpeningTag(metadata, "{}");

        tag.ShouldContain("&amp;");
        tag.ShouldNotContain("?a=1&b=2\""); // Raw & should be escaped
    }

    [Fact]
    public void GenerateOpeningTagShouldThrowWhenMetadataIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            IslandRenderer.GenerateOpeningTag(null!, "{}"));
    }

    [Fact]
    public void GenerateOpeningTagShouldThrowWhenSerializedPropsIsNull()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);

        Should.Throw<ArgumentNullException>(() =>
            IslandRenderer.GenerateOpeningTag(metadata, null!));
    }

    // ── RenderIslandAsync<TComponent> tests ──

    [Fact]
    public async Task RenderIslandAsyncShouldWrapComponentOutputInIslandElement()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/components/Counter.js", ClientDirectiveType.Load)
        {
            DisplayName = "Counter"
        };
        var props = new Dictionary<string, object?> { ["count"] = 5 };

        await IslandRenderer.RenderIslandAsync<TestCounter>(
            dest, metadata, props, SlotCollection.Empty);

        var output = dest.GetOutput();
        output.ShouldStartWith("<atoll-island");
        output.ShouldEndWith("</atoll-island>");
        output.ShouldContain("<div>Count: 5</div>");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldIncludeClientDirective()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Idle)
        {
            DisplayName = "Test"
        };
        var props = new Dictionary<string, object?>();

        await IslandRenderer.RenderIslandAsync<SimpleIsland>(
            dest, metadata, props, SlotCollection.Empty);

        var output = dest.GetOutput();
        output.ShouldContain("client=\"idle\"");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldSerializePropsIntoAttribute()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load)
        {
            DisplayName = "Test"
        };
        var props = new Dictionary<string, object?> { ["title"] = "Hello" };

        await IslandRenderer.RenderIslandAsync<SimpleIsland>(
            dest, metadata, props, SlotCollection.Empty);

        var output = dest.GetOutput();
        output.ShouldContain("props=\"");
        output.ShouldContain("Hello");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldIncludeSsrAttribute()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);
        var props = new Dictionary<string, object?>();

        await IslandRenderer.RenderIslandAsync<SimpleIsland>(
            dest, metadata, props, SlotCollection.Empty);

        var output = dest.GetOutput();
        output.ShouldContain(" ssr>");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldPassSlotsToComponent()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);
        var props = new Dictionary<string, object?>();
        var slots = new SlotBuilder()
            .Default(RenderFragment.FromHtml("<span>Slot content</span>"))
            .Build();

        await IslandRenderer.RenderIslandAsync<SlottedIsland>(
            dest, metadata, props, slots);

        var output = dest.GetOutput();
        output.ShouldContain("<span>Slot content</span>");
    }

    // ── RenderIslandAsync(Type) tests ──

    [Fact]
    public async Task RenderIslandAsyncByTypeShouldWrapComponentOutput()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);
        var props = new Dictionary<string, object?>();

        await IslandRenderer.RenderIslandAsync(
            dest, metadata, typeof(SimpleIsland), props, SlotCollection.Empty);

        var output = dest.GetOutput();
        output.ShouldStartWith("<atoll-island");
        output.ShouldEndWith("</atoll-island>");
        output.ShouldContain("<div>Simple Island</div>");
    }

    [Fact]
    public async Task RenderIslandAsyncByTypeShouldThrowForNonComponent()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);
        var props = new Dictionary<string, object?>();

        await Should.ThrowAsync<ArgumentException>(async () =>
            await IslandRenderer.RenderIslandAsync(
                dest, metadata, typeof(string), props, SlotCollection.Empty));
    }

    // ── Null argument validation tests ──

    [Fact]
    public async Task RenderIslandAsyncGenericShouldThrowForNullDestination()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);

        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await IslandRenderer.RenderIslandAsync<SimpleIsland>(
                null!, metadata, new Dictionary<string, object?>(), SlotCollection.Empty));
    }

    [Fact]
    public async Task RenderIslandAsyncGenericShouldThrowForNullMetadata()
    {
        var dest = new StringRenderDestination();

        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await IslandRenderer.RenderIslandAsync<SimpleIsland>(
                dest, null!, new Dictionary<string, object?>(), SlotCollection.Empty));
    }

    [Fact]
    public async Task RenderIslandAsyncByTypeShouldThrowForNullComponentType()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);

        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await IslandRenderer.RenderIslandAsync(
                dest, metadata, null!, new Dictionary<string, object?>(), SlotCollection.Empty));
    }

    // ── IslandMetadata tests ──

    [Fact]
    public void IslandMetadataShouldThrowForNullComponentUrl()
    {
        Should.Throw<ArgumentNullException>(() =>
            new IslandMetadata(null!, ClientDirectiveType.Load));
    }

    [Fact]
    public void IslandMetadataShouldThrowForEmptyComponentUrl()
    {
        Should.Throw<ArgumentException>(() =>
            new IslandMetadata("", ClientDirectiveType.Load));
    }

    [Fact]
    public void IslandMetadataShouldThrowForWhitespaceComponentUrl()
    {
        Should.Throw<ArgumentException>(() =>
            new IslandMetadata("   ", ClientDirectiveType.Load));
    }

    [Fact]
    public void IslandMetadataShouldStoreComponentUrl()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);

        metadata.ComponentUrl.ShouldBe("/c.js");
    }

    [Fact]
    public void IslandMetadataShouldStoreDirectiveType()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Visible);

        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
    }

    [Fact]
    public void IslandMetadataShouldDefaultExportToDefault()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);

        metadata.ComponentExport.ShouldBe("default");
    }

    [Fact]
    public void IslandMetadataShouldDefaultDisplayNameToUnknown()
    {
        var metadata = new IslandMetadata("/c.js", ClientDirectiveType.Load);

        metadata.DisplayName.ShouldBe("unknown");
    }

    // ── HydrationScriptGenerator tests ──

    [Fact]
    public void GenerateBootstrapScriptShouldReturnExternalScriptTag()
    {
        var html = HydrationScriptGenerator.GenerateBootstrapScript("/scripts/atoll-island.js");

        html.ShouldContain("<script type=\"module\"");
        html.ShouldContain("src=\"/scripts/atoll-island.js\"");
        html.ShouldContain("</script>");
    }

    [Fact]
    public void GenerateBootstrapScriptShouldReturnInlineScriptWhenUrlIsNull()
    {
        var html = HydrationScriptGenerator.GenerateBootstrapScript(null);

        html.ShouldContain("<script type=\"module\">");
        html.ShouldContain("atoll-island");
        html.ShouldContain("</script>");
        html.ShouldNotContain("src=");
    }

    [Fact]
    public void GenerateBootstrapScriptShouldEscapeHtmlInUrl()
    {
        var html = HydrationScriptGenerator.GenerateBootstrapScript("/scripts/island.js?v=1&t=2");

        html.ShouldContain("&amp;");
    }

    [Fact]
    public void GenerateDirectiveScriptShouldReturnScriptTag()
    {
        var html = HydrationScriptGenerator.GenerateDirectiveScript("/scripts/load-directive.js");

        html.ShouldContain("<script type=\"module\"");
        html.ShouldContain("src=\"/scripts/load-directive.js\"");
    }

    [Fact]
    public void GenerateDirectiveScriptShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            HydrationScriptGenerator.GenerateDirectiveScript(null!));
    }

    [Fact]
    public void BootstrapScriptKeyShouldBeNonEmpty()
    {
        HydrationScriptGenerator.BootstrapScriptKey.ShouldNotBeNullOrWhiteSpace();
    }

    // ── Closing tag test ──

    [Fact]
    public void ClosingTagShouldBeCorrect()
    {
        IslandRenderer.ClosingTag.ShouldBe("</atoll-island>");
    }

    // ── End-to-end integration: island with props and directive ──

    [Fact]
    public async Task ShouldRenderCompleteIslandWithAllAttributes()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/components/Counter.js", ClientDirectiveType.Media)
        {
            ComponentExport = "Counter",
            DisplayName = "Counter",
            DirectiveValue = "(max-width: 768px)",
            BeforeHydrationUrl = "/scripts/before.js"
        };
        var props = new Dictionary<string, object?> { ["count"] = 10 };

        await IslandRenderer.RenderIslandAsync<TestCounter>(
            dest, metadata, props, SlotCollection.Empty);

        var output = dest.GetOutput();

        // Verify structure
        output.ShouldStartWith("<atoll-island");
        output.ShouldEndWith("</atoll-island>");

        // Verify all attributes
        output.ShouldContain("component-url=\"/components/Counter.js\"");
        output.ShouldContain("component-export=\"Counter\"");
        output.ShouldContain("client=\"media\"");
        output.ShouldContain("props=\"");
        output.ShouldContain("ssr");
        output.ShouldContain("before-hydration-url=\"/scripts/before.js\"");

        // Verify SSR content
        output.ShouldContain("<div>Count: 10</div>");
    }

    // ── Test component fixtures ──

    private sealed class SimpleIsland : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>Simple Island</div>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestCounter : AtollComponent
    {
        [Parameter]
        public int Count { get; set; }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<div>Count: {Count}</div>");
            return Task.CompletedTask;
        }
    }

    private sealed class SlottedIsland : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }
}

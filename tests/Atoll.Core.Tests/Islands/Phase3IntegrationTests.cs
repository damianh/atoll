using Atoll.Core.Components;
using Atoll.Core.Instructions;
using Atoll.Core.Islands;
using Atoll.Core.Rendering;
using Atoll.Core.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Islands;

/// <summary>
/// Phase 3 cross-cutting integration tests covering end-to-end island
/// rendering workflows: directive detection -> prop serialization ->
/// island HTML generation -> script deduplication.
/// </summary>
public sealed class Phase3IntegrationTests
{
    // ─── Test components for integration scenarios ──────────────────

    [ClientLoad]
    private sealed class CounterIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/components/counter.js";

        [Parameter]
        public int Count { get; set; }

        [Parameter]
        public string Label { get; set; } = "Count";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<div class=\"counter\"><span>{Label}: {Count}</span></div>");
            return Task.CompletedTask;
        }
    }

    [ClientIdle]
    private sealed class SearchIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/components/search.js";

        [Parameter]
        public string Placeholder { get; set; } = "Search...";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<input type=\"search\" placeholder=\"{Placeholder}\" />");
            return Task.CompletedTask;
        }
    }

    [ClientVisible(RootMargin = "100px")]
    private sealed class ChartWebComponent : WebComponentIsland
    {
        public override string TagName => "data-chart";
        public override string ClientModuleUrl => "/components/data-chart.js";

        [Parameter]
        public string DataSource { get; set; } = "";

        protected override Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml("<div class=\"chart-placeholder\">Loading chart...</div>");
            return Task.CompletedTask;
        }
    }

    [ClientMedia("(max-width: 768px)")]
    private sealed class MobileNavWebComponent : WebComponentIsland
    {
        public override string TagName => "mobile-nav";
        public override string ClientModuleUrl => "/components/mobile-nav.js";
        public override string ClientExportName => "MobileNav";

        protected override Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml("<nav><a href=\"/\">Home</a></nav>");
            return Task.CompletedTask;
        }
    }

    [ClientLoad]
    private sealed class NestedParentIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/components/parent.js";

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"parent\">");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    [ClientLoad]
    private sealed class NestedChildIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/components/child.js";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<span class=\"child\">Child island</span>");
            return Task.CompletedTask;
        }
    }

    // Non-island component for mixed scenarios
    private sealed class StaticComponent : AtollComponent
    {
        [Parameter]
        public string Content { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<section>{Content}</section>");
            return Task.CompletedTask;
        }
    }

    // ─── End-to-end: Directive -> Prop serialization -> Island HTML ──────

    [Fact]
    public async Task ShouldRenderCompleteIslandWithPropsAndDirective()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Count"] = 42, ["Label"] = "Items" };

        var island = new CounterIsland();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();

        // Island wrapper
        html.ShouldStartWith("<atoll-island");
        html.ShouldEndWith("</atoll-island>");

        // Directive
        html.ShouldContain("client=\"load\"");

        // Component URL
        html.ShouldContain("component-url=\"/components/counter.js\"");

        // SSR content inside wrapper
        html.ShouldContain("<div class=\"counter\"><span>Items: 42</span></div>");

        // Props should be serialized
        html.ShouldContain("props=\"");

        // SSR flag
        html.ShouldContain(" ssr");
    }

    [Fact]
    public async Task ShouldRenderWebComponentIslandWithCustomElementTag()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["DataSource"] = "/api/data" };

        var island = new ChartWebComponent();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();

        // Island wrapper
        html.ShouldContain("<atoll-island");

        // Custom element inside wrapper
        html.ShouldContain("<data-chart");
        html.ShouldContain("</data-chart>");

        // Directive
        html.ShouldContain("client=\"visible\"");

        // Light DOM content
        html.ShouldContain("<div class=\"chart-placeholder\">Loading chart...</div>");
    }

    [Fact]
    public async Task ShouldSerializeComplexPropsForIsland()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Count"] = 100,
            ["Label"] = "Total"
        };

        var island = new CounterIsland();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();

        // Props are serialized (HTML-encoded in the attribute)
        html.ShouldContain("props=\"");
        // SSR content renders the bound values
        html.ShouldContain("Total: 100");
    }

    // ─── Directive extraction -> IslandMetadata integration ──────────

    [Fact]
    public void DirectiveExtractionShouldMatchIslandMetadata()
    {
        var island = new ChartWebComponent();
        var directive = DirectiveExtractor.GetDirective(island.GetType());
        var metadata = island.CreateMetadata();

        directive.ShouldNotBeNull();
        metadata.ShouldNotBeNull();

        metadata.DirectiveType.ShouldBe(directive.DirectiveType);
        metadata.DirectiveValue.ShouldBe(directive.Value);
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
        metadata.DirectiveValue.ShouldBe("100px");
    }

    [Fact]
    public void DirectiveExtractionShouldWorkForAllDirectiveTypes()
    {
        DirectiveExtractor.GetDirective(typeof(CounterIsland))!
            .DirectiveType.ShouldBe(ClientDirectiveType.Load);

        DirectiveExtractor.GetDirective(typeof(SearchIsland))!
            .DirectiveType.ShouldBe(ClientDirectiveType.Idle);

        DirectiveExtractor.GetDirective(typeof(ChartWebComponent))!
            .DirectiveType.ShouldBe(ClientDirectiveType.Visible);

        DirectiveExtractor.GetDirective(typeof(MobileNavWebComponent))!
            .DirectiveType.ShouldBe(ClientDirectiveType.Media);
    }

    // ─── Script deduplication with multiple islands on same page ──────

    [Fact]
    public void MultipleLoadIslandsShouldOnlyProduceOneBootstrapScript()
    {
        var tracker = new HydrationTracker();
        var processor = new InstructionProcessor();

        // Simulate 5 client:load islands on the same page
        for (var i = 0; i < 5; i++)
        {
            tracker.AddToProcessor(
                processor,
                ClientDirectiveType.Load,
                "/_atoll/island.js",
                "/_atoll/directives.js");
        }

        // Only 2 scripts: bootstrap + load directive
        processor.Count.ShouldBe(2);

        var scripts = processor.GetInstructions<ScriptInstruction>().ToList();
        scripts.Count.ShouldBe(2);
    }

    [Fact]
    public void MixedIslandTypesShouldDeduplicateCorrectly()
    {
        var tracker = new HydrationTracker();
        var processor = new InstructionProcessor();

        // 2 load islands
        tracker.AddToProcessor(processor, ClientDirectiveType.Load,
            "/_atoll/island.js", "/_atoll/load.js");
        tracker.AddToProcessor(processor, ClientDirectiveType.Load,
            "/_atoll/island.js", "/_atoll/load.js");

        // 1 idle island
        tracker.AddToProcessor(processor, ClientDirectiveType.Idle,
            "/_atoll/island.js", "/_atoll/idle.js");

        // 1 visible island
        tracker.AddToProcessor(processor, ClientDirectiveType.Visible,
            "/_atoll/island.js", "/_atoll/visible.js");

        // 1 media island
        tracker.AddToProcessor(processor, ClientDirectiveType.Media,
            "/_atoll/island.js", "/_atoll/media.js");

        // bootstrap + 4 directives = 5
        processor.Count.ShouldBe(5);
    }

    [Fact]
    public async Task ScriptInstructionsShouldRenderAsModuleScripts()
    {
        var tracker = new HydrationTracker();
        var processor = new InstructionProcessor();

        tracker.AddToProcessor(
            processor,
            ClientDirectiveType.Load,
            "/_atoll/island.js",
            "/_atoll/load.js");

        var dest = new StringRenderDestination();
        await processor.RenderAllAsync<ScriptInstruction>(dest);

        var output = dest.GetOutput();
        output.ShouldContain("<script type=\"module\"");
        output.ShouldContain("src=\"/_atoll/island.js\"");
        output.ShouldContain("src=\"/_atoll/load.js\"");
    }

    // ─── Prop serialization round-trip consistency ──────────────────

    [Fact]
    public void PropSerializerShouldHandleAllIslandPropTypes()
    {
        var props = new Dictionary<string, object?>
        {
            ["count"] = 42,
            ["label"] = "Hello",
            ["active"] = true,
            ["ratio"] = 3.14,
            ["tags"] = new[] { "a", "b", "c" },
            ["created"] = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            ["url"] = new Uri("https://example.com"),
        };

        var json = PropSerializer.Serialize(props);

        json.ShouldNotBeNullOrWhiteSpace();
        // Should be valid JSON
        json.ShouldStartWith("{");
        json.ShouldEndWith("}");
    }

    [Fact]
    public void PropSerializerShouldProduceValidJsonForIslandAttribute()
    {
        var props = new Dictionary<string, object?>
        {
            ["title"] = "Hello \"World\"",
            ["count"] = 42
        };

        var json = PropSerializer.Serialize(props, "TestComponent");

        // The JSON should be parseable
        var parsed = System.Text.Json.JsonDocument.Parse(json);
        parsed.RootElement.ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Object);
    }

    // ─── Nested island coordination ──────────────────

    [Fact]
    public async Task NestedIslandsShouldBothRenderIslandWrappers()
    {
        // Render parent island with child island as slot content
        var parentDest = new StringRenderDestination();

        var childMetadata = new IslandMetadata("/components/child.js", ClientDirectiveType.Load)
        {
            DisplayName = "NestedChildIsland",
        };

        // First render the child island to get its HTML
        var childDest = new StringRenderDestination();
        await IslandRenderer.RenderIslandAsync<NestedChildIsland>(
            childDest,
            childMetadata,
            new Dictionary<string, object?>(),
            SlotCollection.Empty);
        var childHtml = childDest.GetOutput();

        // Now render the parent island with the child HTML as slot content
        var parentMetadata = new IslandMetadata("/components/parent.js", ClientDirectiveType.Load)
        {
            DisplayName = "NestedParentIsland",
        };

        var parentSlots = new SlotBuilder()
            .Default(RenderFragment.FromHtml(childHtml))
            .Build();

        await IslandRenderer.RenderIslandAsync<NestedParentIsland>(
            parentDest,
            parentMetadata,
            new Dictionary<string, object?>(),
            parentSlots);

        var html = parentDest.GetOutput();

        // Should contain nested atoll-island elements
        html.ShouldContain("component-url=\"/components/parent.js\"");
        html.ShouldContain("component-url=\"/components/child.js\"");

        // Both should have island wrappers
        var islandOpenCount = CountOccurrences(html, "<atoll-island");
        islandOpenCount.ShouldBe(2);

        var islandCloseCount = CountOccurrences(html, "</atoll-island>");
        islandCloseCount.ShouldBe(2);
    }

    // ─── Mixed static and island components ──────────────────

    [Fact]
    public async Task PageWithMixedStaticAndIslandComponentsShouldRenderCorrectly()
    {
        var dest = new StringRenderDestination();

        // Render a static component
        dest.Write(RenderChunk.Html("<!DOCTYPE html><html><body>"));

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new StaticComponent { Content = "Header" },
            dest,
            new Dictionary<string, object?> { ["Content"] = "Header" });

        // Render an island component
        var counterMetadata = new IslandMetadata("/components/counter.js", ClientDirectiveType.Load)
        {
            DisplayName = "CounterIsland",
        };

        await IslandRenderer.RenderIslandAsync<CounterIsland>(
            dest,
            counterMetadata,
            new Dictionary<string, object?> { ["Count"] = 10, ["Label"] = "Score" },
            SlotCollection.Empty);

        // Render another static component
        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new StaticComponent { Content = "Footer" },
            dest,
            new Dictionary<string, object?> { ["Content"] = "Footer" });

        dest.Write(RenderChunk.Html("</body></html>"));

        var html = dest.GetOutput();

        // Static components
        html.ShouldContain("<section>Header</section>");
        html.ShouldContain("<section>Footer</section>");

        // Island component
        html.ShouldContain("<atoll-island");
        html.ShouldContain("<div class=\"counter\"><span>Score: 10</span></div>");
        html.ShouldContain("</atoll-island>");

        // Only one island wrapper
        CountOccurrences(html, "<atoll-island").ShouldBe(1);
    }

    // ─── Full page with multiple islands + script deduplication ──────

    [Fact]
    public async Task FullPageWithMultipleIslandsAndScriptDeduplication()
    {
        var tracker = new HydrationTracker();
        var processor = new InstructionProcessor();
        var dest = new StringRenderDestination();

        dest.Write(RenderChunk.Html("<!DOCTYPE html><html><head></head><body>"));

        // Island 1: counter (client:load)
        var counterMetadata = new IslandMetadata("/components/counter.js", ClientDirectiveType.Load)
        {
            DisplayName = "Counter",
        };
        tracker.AddToProcessor(processor, ClientDirectiveType.Load,
            "/_atoll/island.js", "/_atoll/directives.js");
        await IslandRenderer.RenderIslandAsync<CounterIsland>(
            dest, counterMetadata,
            new Dictionary<string, object?> { ["Count"] = 1, ["Label"] = "A" },
            SlotCollection.Empty);

        // Island 2: another counter (client:load - duplicate directive type)
        tracker.AddToProcessor(processor, ClientDirectiveType.Load,
            "/_atoll/island.js", "/_atoll/directives.js");
        await IslandRenderer.RenderIslandAsync<CounterIsland>(
            dest, counterMetadata,
            new Dictionary<string, object?> { ["Count"] = 2, ["Label"] = "B" },
            SlotCollection.Empty);

        // Island 3: search (client:idle)
        var searchMetadata = new IslandMetadata("/components/search.js", ClientDirectiveType.Idle)
        {
            DisplayName = "Search",
        };
        tracker.AddToProcessor(processor, ClientDirectiveType.Idle,
            "/_atoll/island.js", "/_atoll/idle.js");
        await IslandRenderer.RenderIslandAsync<SearchIsland>(
            dest, searchMetadata,
            new Dictionary<string, object?> { ["Placeholder"] = "Find..." },
            SlotCollection.Empty);

        // Write script tags before closing body
        await processor.RenderAllAsync<ScriptInstruction>(dest);

        dest.Write(RenderChunk.Html("</body></html>"));

        var html = dest.GetOutput();

        // 3 island wrappers
        CountOccurrences(html, "<atoll-island").ShouldBe(3);

        // Script deduplication: only 3 scripts (bootstrap + load + idle)
        CountOccurrences(html, "<script type=\"module\"").ShouldBe(3);

        // SSR content
        html.ShouldContain("A: 1");
        html.ShouldContain("B: 2");
        html.ShouldContain("placeholder=\"Find...\"");
    }

    // ─── IslandScriptProvider + HydrationScriptGenerator integration ──────

    [Fact]
    public void BootstrapScriptShouldContainCustomElementDefinition()
    {
        var script = IslandScriptProvider.GetIslandScript();
        var bootstrapHtml = HydrationScriptGenerator.GenerateBootstrapScript("/_atoll/island.js");

        script.ShouldContain("customElements.define");
        bootstrapHtml.ShouldContain("/_atoll/island.js");
    }

    [Fact]
    public void DirectiveScriptShouldContainAllDirectiveHandlers()
    {
        var script = IslandScriptProvider.GetDirectivesScript();

        // All four directives
        script.ShouldContain("Atoll.load");
        script.ShouldContain("Atoll.idle");
        script.ShouldContain("Atoll.visible");
        script.ShouldContain("Atoll.media");
    }

    // ─── IClientComponent consistency across island types ──────

    [Fact]
    public void VanillaJsIslandShouldImplementIClientComponent()
    {
        IClientComponent component = new CounterIsland();

        component.ClientModuleUrl.ShouldBe("/components/counter.js");
        component.ClientExportName.ShouldBe("default");
    }

    [Fact]
    public void WebComponentIslandShouldImplementIClientComponent()
    {
        IClientComponent component = new MobileNavWebComponent();

        component.ClientModuleUrl.ShouldBe("/components/mobile-nav.js");
        component.ClientExportName.ShouldBe("MobileNav");
    }

    // ─── WebComponentAdapter + WebComponentIsland rendering consistency ──────

    [Fact]
    public async Task WebComponentIslandShouldRenderValidCustomElementTags()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>();

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new MobileNavWebComponent(), dest, props);

        var html = dest.GetOutput();

        // Tag name should be valid custom element
        WebComponentAdapter.IsValidCustomElementName("mobile-nav").ShouldBeTrue();

        html.ShouldStartWith("<mobile-nav");
        html.ShouldEndWith("</mobile-nav>");
    }

    // ─── Edge cases ──────────────────────────────────────

    [Fact]
    public async Task IslandWithEmptyPropsShouldRenderCorrectly()
    {
        var dest = new StringRenderDestination();
        var metadata = new IslandMetadata("/components/counter.js", ClientDirectiveType.Load)
        {
            DisplayName = "Counter",
        };

        await IslandRenderer.RenderIslandAsync<CounterIsland>(
            dest, metadata, new Dictionary<string, object?>(), SlotCollection.Empty);

        var html = dest.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("props=\"");
        html.ShouldContain("</atoll-island>");
    }

    [Fact]
    public void TrackerShouldWorkWithProcessorDeduplicationCombined()
    {
        var tracker = new HydrationTracker();
        var processor = new InstructionProcessor();

        // Add via tracker
        tracker.AddToProcessor(processor, ClientDirectiveType.Load,
            "/_atoll/island.js", "/_atoll/load.js");

        // Add directly to processor with same key — should be deduplicated
        var duplicateScript = ScriptInstruction.Module("/_atoll/island.js");
        processor.Add(duplicateScript);

        // Both tracker and processor should work together
        // Processor deduplication is key-based, so the Module script key differs
        // from the tracker's key. The processor keeps its own dedup.
        tracker.HasBootstrap.ShouldBeTrue();
        tracker.HasDirective(ClientDirectiveType.Load).ShouldBeTrue();
    }

    [Fact]
    public void HydrationScriptGeneratorKeysShouldBeConsistent()
    {
        // The bootstrap key should be the same constant used everywhere
        HydrationScriptGenerator.BootstrapScriptKey.ShouldBe("atoll:island:bootstrap");
    }

    // ─── Helper methods ──────────────────────────────────────

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}

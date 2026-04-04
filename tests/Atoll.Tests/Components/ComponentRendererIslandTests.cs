using Atoll.Components;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Components;

/// <summary>
/// Tests that <see cref="ComponentRenderer"/> automatically detects island components
/// (those implementing <see cref="IClientComponent"/> with a client directive attribute)
/// and routes them through <see cref="IslandRenderer"/> to produce the
/// <c>&lt;atoll-island&gt;</c> wrapper.
/// </summary>
public sealed class ComponentRendererIslandTests
{
    // ─── Test fixtures ────────────────────────────────────────────

    [ClientLoad]
    private sealed class LoadIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/load-island.js";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"load-island\">Load content</div>");
            return Task.CompletedTask;
        }
    }

    [ClientIdle]
    private sealed class IdleIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/idle-island.js";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<span>Idle content</span>");
            return Task.CompletedTask;
        }
    }

    [ClientVisible]
    private sealed class VisibleIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/visible-island.js";
        public override string ClientExportName => "VisibleWidget";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<section>Visible content</section>");
            return Task.CompletedTask;
        }
    }

    [ClientLoad]
    private sealed class WebIsland : WebComponentIsland
    {
        public override string TagName => "test-widget";
        public override string ClientModuleUrl => "/components/test-widget.js";

        protected override Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml("<p>Web component content</p>");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Implements <see cref="IClientComponent"/> but has NO directive attribute.
    /// This tests graceful fallback to plain rendering.
    /// </summary>
    private sealed class NoDirectiveIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/no-directive.js";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>No directive content</div>");
            return Task.CompletedTask;
        }
    }

    private sealed class PlainComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Plain content</p>");
            return Task.CompletedTask;
        }
    }

    // ─── Task 5: Island components get wrapper via ComponentRenderer ──────

    [Fact]
    public async Task RenderComponentAsyncGenericShouldWrapVanillaJsIslandWithLoadDirective()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<LoadIsland>(destination);

        var html = destination.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
    }

    [Fact]
    public async Task RenderComponentAsyncGenericShouldProduceCorrectComponentUrl()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<LoadIsland>(destination);

        var html = destination.GetOutput();
        html.ShouldContain("component-url=\"/scripts/load-island.js\"");
    }

    [Fact]
    public async Task RenderComponentAsyncGenericShouldProduceCorrectClientDirective()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<IdleIsland>(destination);

        var html = destination.GetOutput();
        html.ShouldContain("client=\"idle\"");
    }

    [Fact]
    public async Task RenderComponentAsyncGenericShouldProduceSsrAttribute()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<LoadIsland>(destination);

        var html = destination.GetOutput();
        html.ShouldContain(" ssr");
    }

    [Fact]
    public async Task RenderComponentAsyncGenericShouldPreserveSsrContentInsideWrapper()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<LoadIsland>(destination);

        var html = destination.GetOutput();
        html.ShouldContain("<div class=\"load-island\">Load content</div>");
    }

    [Fact]
    public async Task RenderComponentAsyncGenericShouldWrapWebComponentIsland()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<WebIsland>(destination);

        var html = destination.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
        html.ShouldContain("component-url=\"/components/test-widget.js\"");
        html.ShouldContain("client=\"load\"");
        html.ShouldContain("<test-widget");
        html.ShouldContain("</test-widget>");
    }

    [Fact]
    public async Task RenderComponentAsyncGenericShouldIncludeCustomExportName()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<VisibleIsland>(destination);

        var html = destination.GetOutput();
        html.ShouldContain("component-export=\"VisibleWidget\"");
    }

    [Fact]
    public async Task RenderComponentAsyncInstanceShouldWrapIslandInstance()
    {
        var destination = new StringRenderDestination();
        var island = new LoadIsland();

        await ComponentRenderer.RenderComponentAsync(island, destination);

        var html = destination.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
        html.ShouldContain("client=\"load\"");
        html.ShouldContain("<div class=\"load-island\">Load content</div>");
    }

    [Fact]
    public async Task RenderComponentAsyncInstanceShouldWrapIslandCastToIAtollComponent()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new IdleIsland(), destination);

        var html = destination.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("client=\"idle\"");
    }

    [Fact]
    public async Task ToFragmentShouldWrapIslandComponent()
    {
        var fragment = ComponentRenderer.ToFragment<LoadIsland>();

        var html = await fragment.RenderToStringAsync();

        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
        html.ShouldContain("component-url=\"/scripts/load-island.js\"");
    }

    // ─── Task 6: Non-island components are unaffected (regression) ────────

    [Fact]
    public async Task RenderComponentAsyncGenericShouldNotWrapPlainComponent()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<PlainComponent>(destination);

        var html = destination.GetOutput();
        html.ShouldNotContain("<atoll-island");
        html.ShouldBe("<p>Plain content</p>");
    }

    [Fact]
    public async Task RenderComponentAsyncInstanceShouldNotWrapPlainComponentInstance()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync(new PlainComponent(), destination);

        var html = destination.GetOutput();
        html.ShouldNotContain("<atoll-island");
        html.ShouldBe("<p>Plain content</p>");
    }

    [Fact]
    public async Task ToFragmentShouldNotWrapPlainComponent()
    {
        var fragment = ComponentRenderer.ToFragment<PlainComponent>();

        var html = await fragment.RenderToStringAsync();

        html.ShouldNotContain("<atoll-island");
        html.ShouldBe("<p>Plain content</p>");
    }

    // ─── Task 7: IClientComponent without directive falls back gracefully ──

    [Fact]
    public async Task RenderComponentAsyncGenericShouldFallBackToPlainRenderingWhenNoDirective()
    {
        var destination = new StringRenderDestination();

        // NoDirectiveIsland implements IClientComponent but has no [ClientLoad] etc. attribute
        await ComponentRenderer.RenderComponentAsync<NoDirectiveIsland>(destination);

        var html = destination.GetOutput();
        html.ShouldNotContain("<atoll-island");
        html.ShouldBe("<div>No directive content</div>");
    }

    [Fact]
    public async Task RenderComponentAsyncInstanceShouldFallBackToPlainRenderingWhenNoDirective()
    {
        var destination = new StringRenderDestination();
        var island = new NoDirectiveIsland();

        await ComponentRenderer.RenderComponentAsync(island, destination);

        var html = destination.GetOutput();
        html.ShouldNotContain("<atoll-island");
        html.ShouldBe("<div>No directive content</div>");
    }

    [Fact]
    public async Task ToFragmentShouldFallBackToPlainRenderingWhenNoDirective()
    {
        var fragment = ComponentRenderer.ToFragment<NoDirectiveIsland>();

        var html = await fragment.RenderToStringAsync();

        html.ShouldNotContain("<atoll-island");
        html.ShouldBe("<div>No directive content</div>");
    }
}

using Atoll.Components;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Islands;

public sealed class VanillaJsIslandTests
{
    // ─── Test components ─────────────────────────────────────────

    [ClientLoad]
    private sealed class CounterIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/counter.js";

        [Parameter]
        public int InitialCount { get; set; }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<div class=\"counter\">{InitialCount}</div>");
            return Task.CompletedTask;
        }
    }

    [ClientIdle]
    private sealed class IdleIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/idle-widget.js";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<span>Idle content</span>");
            return Task.CompletedTask;
        }
    }

    [ClientVisible(RootMargin = "200px")]
    private sealed class LazyImageIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/lazy-image.js";
        public override string ClientExportName => "LazyImage";

        [Parameter]
        public string Src { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<img src=\"{Src}\" loading=\"lazy\" />");
            return Task.CompletedTask;
        }
    }

    [ClientMedia("(max-width: 768px)")]
    private sealed class MobileMenuIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/mobile-menu.js";
        public override string ClientExportName => "MobileMenu.init";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<nav class=\"mobile-menu\">Menu</nav>");
            return Task.CompletedTask;
        }
    }

    private sealed class NoDirectiveIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/no-directive.js";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>No directive</div>");
            return Task.CompletedTask;
        }
    }

    [ClientLoad]
    private sealed class AsyncIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/async.js";

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>");
            await Task.Yield();
            WriteHtml("Async content");
            WriteHtml("</div>");
        }
    }

    [ClientLoad]
    private sealed class SlottedIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/slotted.js";

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"wrapper\">");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    // ─── IClientComponent interface tests ─────────────────────────────────

    [Fact]
    public void ClientModuleUrlShouldReturnConfiguredUrl()
    {
        var island = new CounterIsland();

        island.ClientModuleUrl.ShouldBe("/scripts/counter.js");
    }

    [Fact]
    public void ClientExportNameShouldDefaultToDefault()
    {
        var island = new CounterIsland();

        island.ClientExportName.ShouldBe("default");
    }

    [Fact]
    public void ClientExportNameShouldReturnCustomExport()
    {
        var island = new LazyImageIsland();

        island.ClientExportName.ShouldBe("LazyImage");
    }

    [Fact]
    public void ClientExportNameShouldSupportDottedPaths()
    {
        var island = new MobileMenuIsland();

        island.ClientExportName.ShouldBe("MobileMenu.init");
    }

    [Fact]
    public void IClientComponentInterfaceShouldBeImplemented()
    {
        var island = new CounterIsland();

        island.ShouldBeAssignableTo<IClientComponent>();
    }

    // ─── CreateMetadata tests ─────────────────────────────────

    [Fact]
    public void CreateMetadataShouldReturnMetadataForLoadDirective()
    {
        var island = new CounterIsland();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.ComponentUrl.ShouldBe("/scripts/counter.js");
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Load);
        metadata.ComponentExport.ShouldBe("default");
        metadata.DirectiveValue.ShouldBeNull();
        metadata.DisplayName.ShouldBe("CounterIsland");
    }

    [Fact]
    public void CreateMetadataShouldReturnMetadataForIdleDirective()
    {
        var island = new IdleIsland();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
    }

    [Fact]
    public void CreateMetadataShouldReturnMetadataForVisibleDirective()
    {
        var island = new LazyImageIsland();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
        metadata.DirectiveValue.ShouldBe("200px");
        metadata.ComponentExport.ShouldBe("LazyImage");
    }

    [Fact]
    public void CreateMetadataShouldReturnMetadataForMediaDirective()
    {
        var island = new MobileMenuIsland();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Media);
        metadata.DirectiveValue.ShouldBe("(max-width: 768px)");
        metadata.ComponentExport.ShouldBe("MobileMenu.init");
    }

    [Fact]
    public void CreateMetadataShouldReturnNullForComponentWithoutDirective()
    {
        var island = new NoDirectiveIsland();

        var metadata = island.CreateMetadata();

        metadata.ShouldBeNull();
    }

    // ─── RenderIslandAsync tests ─────────────────────────────────

    [Fact]
    public async Task RenderIslandAsyncShouldWrapSsrOutputInIslandElement()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["InitialCount"] = 42 };

        var island = new CounterIsland();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
        html.ShouldContain("component-url=\"/scripts/counter.js\"");
        html.ShouldContain("client=\"load\"");
        html.ShouldContain("<div class=\"counter\">42</div>");
        html.ShouldContain("ssr");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldIncludeSerializedProps()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["InitialCount"] = 42 };

        var island = new CounterIsland();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();
        html.ShouldContain("props=\"");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldIncludeComponentExport()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Src"] = "/photo.jpg" };

        var island = new LazyImageIsland();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();
        html.ShouldContain("component-export=\"LazyImage\"");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldIncludeDirectiveValue()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>();

        var island = new MobileMenuIsland();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();
        html.ShouldContain("client=\"media\"");
        // The opts attribute should contain the media query value
        html.ShouldContain("opts=\"");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldThrowWithoutDirective()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>();

        var island = new NoDirectiveIsland();

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => island.RenderIslandAsync(dest, props));
        ex.Message.ShouldContain("does not have a client directive attribute");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldWorkWithNoPropsOverload()
    {
        var dest = new StringRenderDestination();

        var island = new IdleIsland();
        await island.RenderIslandAsync(dest);

        var html = dest.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("<span>Idle content</span>");
        html.ShouldContain("client=\"idle\"");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldHandleAsyncContent()
    {
        var dest = new StringRenderDestination();

        var island = new AsyncIsland();
        await island.RenderIslandAsync(dest);

        var html = dest.GetOutput();
        html.ShouldContain("<div>Async content</div>");
        html.ShouldContain("<atoll-island");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldHandleSlots()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>();
        var slots = new SlotBuilder()
            .Default(RenderFragment.FromHtml("<p>Slot content</p>"))
            .Build();

        var island = new SlottedIsland();
        await island.RenderIslandAsync(dest, props, slots);

        var html = dest.GetOutput();
        html.ShouldContain("<div class=\"wrapper\">");
        html.ShouldContain("<p>Slot content</p>");
        html.ShouldContain("</atoll-island>");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldThrowForNullDestination()
    {
        var island = new CounterIsland();

        await Should.ThrowAsync<ArgumentNullException>(
            () => island.RenderIslandAsync(null!, new Dictionary<string, object?>()));
    }

    [Fact]
    public async Task RenderIslandAsyncShouldThrowForNullProps()
    {
        var island = new CounterIsland();

        await Should.ThrowAsync<ArgumentNullException>(
            () => island.RenderIslandAsync(new StringRenderDestination(), null!));
    }

    [Fact]
    public async Task RenderIslandAsyncShouldThrowForNullSlots()
    {
        var island = new CounterIsland();

        await Should.ThrowAsync<ArgumentNullException>(
            () => island.RenderIslandAsync(
                new StringRenderDestination(),
                new Dictionary<string, object?>(),
                null!));
    }

    // ─── Regular component rendering (non-island) tests ─────────────────

    [Fact]
    public async Task ShouldRenderAsRegularComponentViaComponentRenderer()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["InitialCount"] = 10 };

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new CounterIsland(), dest, props);

        var html = dest.GetOutput();
        // When rendered as a regular component, no island wrapper
        html.ShouldBe("<div class=\"counter\">10</div>");
    }

    [Fact]
    public void ShouldBeAssignableToIAtollComponent()
    {
        var island = new CounterIsland();

        island.ShouldBeAssignableTo<IAtollComponent>();
    }

    // ─── Metadata + island rendering integration ─────────────────

    [Fact]
    public async Task ShouldRenderWithAllDirectiveTypes()
    {
        // Load
        var loadDest = new StringRenderDestination();
        await new CounterIsland().RenderIslandAsync(loadDest);
        loadDest.GetOutput().ShouldContain("client=\"load\"");

        // Idle
        var idleDest = new StringRenderDestination();
        await new IdleIsland().RenderIslandAsync(idleDest);
        idleDest.GetOutput().ShouldContain("client=\"idle\"");

        // Visible
        var visibleDest = new StringRenderDestination();
        await new LazyImageIsland().RenderIslandAsync(visibleDest);
        visibleDest.GetOutput().ShouldContain("client=\"visible\"");

        // Media
        var mediaDest = new StringRenderDestination();
        await new MobileMenuIsland().RenderIslandAsync(mediaDest);
        mediaDest.GetOutput().ShouldContain("client=\"media\"");
    }

    [Fact]
    public async Task ShouldPreserveDisplayNameInMetadata()
    {
        var dest = new StringRenderDestination();

        await new CounterIsland().RenderIslandAsync(dest);

        // DisplayName is used in opts attribute
        var html = dest.GetOutput();
        html.ShouldContain("CounterIsland");
    }

    [Fact]
    public void CreateMetadataShouldSetDisplayNameToTypeName()
    {
        var metadata = new LazyImageIsland().CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DisplayName.ShouldBe("LazyImageIsland");
    }

    [Fact]
    public async Task RenderIslandAsyncWithPropsOnlyShouldUseEmptySlots()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["InitialCount"] = 5 };

        var island = new CounterIsland();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();
        html.ShouldContain("<div class=\"counter\">5</div>");
    }
}

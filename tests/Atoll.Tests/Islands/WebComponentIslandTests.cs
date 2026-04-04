using Atoll.Components;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Islands;

public sealed class WebComponentIslandTests
{
    // ─── Test components ─────────────────────────────────────────

    [ClientLoad]
    private sealed class MyCounter : WebComponentIsland
    {
        public override string TagName => "my-counter";
        public override string ClientModuleUrl => "/components/my-counter.js";

        [Parameter]
        public int Count { get; set; }

        protected override Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml($"<span class=\"count\">{Count}</span>");
            return Task.CompletedTask;
        }
    }

    [ClientIdle]
    private sealed class AppHeader : WebComponentIsland
    {
        public override string TagName => "app-header";
        public override string ClientModuleUrl => "/components/app-header.js";
        public override string ClientExportName => "AppHeader";

        [Parameter]
        public string Title { get; set; } = "";

        protected override Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml($"<h1>{Title}</h1>");
            return Task.CompletedTask;
        }
    }

    [ClientVisible]
    private sealed class LazyChart : WebComponentIsland
    {
        public override string TagName => "lazy-chart";
        public override string ClientModuleUrl => "/components/lazy-chart.js";
        protected override bool RenderPropsAsAttributes => false;

        [Parameter]
        public string DataUrl { get; set; } = "";

        protected override Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml("<div class=\"chart-placeholder\">Loading chart...</div>");
            return Task.CompletedTask;
        }
    }

    [ClientMedia("(prefers-color-scheme: dark)")]
    private sealed class DarkModeToggle : WebComponentIsland
    {
        public override string TagName => "dark-mode-toggle";
        public override string ClientModuleUrl => "/components/dark-mode.js";

        protected override Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml("<button>Toggle</button>");
            return Task.CompletedTask;
        }
    }

    private sealed class NoDirectiveWidget : WebComponentIsland
    {
        public override string TagName => "no-directive-widget";
        public override string ClientModuleUrl => "/components/widget.js";

        protected override Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml("<p>Widget</p>");
            return Task.CompletedTask;
        }
    }

    [ClientLoad]
    private sealed class AsyncWidget : WebComponentIsland
    {
        public override string TagName => "async-widget";
        public override string ClientModuleUrl => "/components/async-widget.js";

        protected override async Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml("<div>");
            await Task.Yield();
            WriteHtml("Async content");
            WriteHtml("</div>");
        }
    }

    [ClientLoad]
    private sealed class SlottedWidget : WebComponentIsland
    {
        public override string TagName => "slotted-widget";
        public override string ClientModuleUrl => "/components/slotted.js";

        protected override async Task RenderLightDomAsync(RenderContext context)
        {
            WriteHtml("<div class=\"inner\">");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    // ─── WebComponentAdapter tests ─────────────────────────────────

    [Theory]
    [InlineData("my-element", true)]
    [InlineData("app-header", true)]
    [InlineData("x-data", true)]
    [InlineData("my-super-long-element-name", true)]
    [InlineData("div", false)]
    [InlineData("span", false)]
    [InlineData("", false)]
    [InlineData("1-element", false)]
    [InlineData("My-Element", false)]
    [InlineData("MY-ELEMENT", false)]
    public void IsValidCustomElementNameShouldValidateCorrectly(string tagName, bool expected)
    {
        WebComponentAdapter.IsValidCustomElementName(tagName).ShouldBe(expected);
    }

    [Fact]
    public void IsValidCustomElementNameShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => WebComponentAdapter.IsValidCustomElementName(null!));
    }

    [Fact]
    public void GenerateOpeningTagShouldRenderTagWithNoProps()
    {
        var tag = WebComponentAdapter.GenerateOpeningTag("my-element");

        tag.ShouldBe("<my-element>");
    }

    [Fact]
    public void GenerateOpeningTagShouldRenderTagWithEmptyProps()
    {
        var props = new Dictionary<string, object?>();

        var tag = WebComponentAdapter.GenerateOpeningTag("my-element", props);

        tag.ShouldBe("<my-element>");
    }

    [Fact]
    public void GenerateOpeningTagShouldRenderStringProps()
    {
        var props = new Dictionary<string, object?>
        {
            ["title"] = "Hello",
            ["data-id"] = "123"
        };

        var tag = WebComponentAdapter.GenerateOpeningTag("my-element", props);

        tag.ShouldContain("title=\"Hello\"");
        tag.ShouldContain("data-id=\"123\"");
    }

    [Fact]
    public void GenerateOpeningTagShouldRenderNumericProps()
    {
        var props = new Dictionary<string, object?>
        {
            ["count"] = 42,
            ["ratio"] = 3.14
        };

        var tag = WebComponentAdapter.GenerateOpeningTag("my-element", props);

        tag.ShouldContain("count=\"42\"");
        tag.ShouldContain("ratio=\"3.14\"");
    }

    [Fact]
    public void GenerateOpeningTagShouldRenderBooleanProps()
    {
        var props = new Dictionary<string, object?>
        {
            ["active"] = true,
            ["disabled"] = false
        };

        var tag = WebComponentAdapter.GenerateOpeningTag("my-element", props);

        tag.ShouldContain("active=\"true\"");
        tag.ShouldContain("disabled=\"false\"");
    }

    [Fact]
    public void GenerateOpeningTagShouldSkipNullProps()
    {
        var props = new Dictionary<string, object?>
        {
            ["title"] = "Hello",
            ["subtitle"] = null
        };

        var tag = WebComponentAdapter.GenerateOpeningTag("my-element", props);

        tag.ShouldContain("title=\"Hello\"");
        tag.ShouldNotContain("subtitle");
    }

    [Fact]
    public void GenerateOpeningTagShouldSkipComplexObjectProps()
    {
        var props = new Dictionary<string, object?>
        {
            ["title"] = "Hello",
            ["config"] = new { Nested = true }
        };

        var tag = WebComponentAdapter.GenerateOpeningTag("my-element", props);

        tag.ShouldContain("title=\"Hello\"");
        tag.ShouldNotContain("config");
    }

    [Fact]
    public void GenerateOpeningTagShouldEscapeAttributeValues()
    {
        var props = new Dictionary<string, object?>
        {
            ["title"] = "Hello & \"World\""
        };

        var tag = WebComponentAdapter.GenerateOpeningTag("my-element", props);

        tag.ShouldContain("&amp;");
        tag.ShouldContain("&quot;");
    }

    [Fact]
    public void GenerateOpeningTagShouldThrowForInvalidTagName()
    {
        Should.Throw<ArgumentException>(
            () => WebComponentAdapter.GenerateOpeningTag("div"));
    }

    [Fact]
    public void GenerateOpeningTagWithPropsShouldThrowForInvalidTagName()
    {
        var props = new Dictionary<string, object?>();

        Should.Throw<ArgumentException>(
            () => WebComponentAdapter.GenerateOpeningTag("div", props));
    }

    [Fact]
    public void GenerateOpeningTagShouldThrowForNullTagName()
    {
        Should.Throw<ArgumentNullException>(
            () => WebComponentAdapter.GenerateOpeningTag(null!));
    }

    [Fact]
    public void GenerateClosingTagShouldReturnClosingTag()
    {
        var tag = WebComponentAdapter.GenerateClosingTag("my-element");

        tag.ShouldBe("</my-element>");
    }

    [Fact]
    public void GenerateClosingTagShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => WebComponentAdapter.GenerateClosingTag(null!));
    }

    // ─── WebComponentIsland rendering tests ─────────────────────────────────

    [Fact]
    public async Task ShouldRenderCustomElementTagDuringSsr()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Count"] = 7 };

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new MyCounter(), dest, props);

        var html = dest.GetOutput();
        // Island components rendered via ComponentRenderer get the <atoll-island> wrapper
        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
        html.ShouldContain("<my-counter");
        html.ShouldContain("</my-counter>");
        html.ShouldContain("<span class=\"count\">7</span>");
    }

    [Fact]
    public async Task ShouldRenderPropsAsAttributesByDefault()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Count"] = 7 };

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new MyCounter(), dest, props);

        var html = dest.GetOutput();
        html.ShouldContain("Count=\"7\"");
    }

    [Fact]
    public async Task ShouldSkipAttributesWhenRenderPropsAsAttributesIsFalse()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["DataUrl"] = "/data.json" };

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new LazyChart(), dest, props);

        var html = dest.GetOutput();
        // Island components rendered via ComponentRenderer get the <atoll-island> wrapper
        html.ShouldContain("<atoll-island");
        html.ShouldContain("<lazy-chart>");
        html.ShouldNotContain("DataUrl=");
    }

    [Fact]
    public async Task ShouldRenderStringPropsAsAttributes()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Title"] = "Hello World" };

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new AppHeader(), dest, props);

        var html = dest.GetOutput();
        html.ShouldContain("Title=\"Hello World\"");
        html.ShouldContain("<h1>Hello World</h1>");
    }

    [Fact]
    public async Task ShouldRenderAsyncLightDomContent()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>();

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new AsyncWidget(), dest, props);

        var html = dest.GetOutput();
        // Island components rendered via ComponentRenderer get the <atoll-island> wrapper
        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
        html.ShouldContain("<async-widget>");
        html.ShouldContain("</async-widget>");
        html.ShouldContain("<div>Async content</div>");
    }

    [Fact]
    public async Task ShouldRenderSlotsInLightDom()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>();
        var slots = new SlotBuilder()
            .Default(RenderFragment.FromHtml("<p>Slotted!</p>"))
            .Build();

        await ComponentRenderer.RenderComponentAsync(
            (IAtollComponent)new SlottedWidget(), dest, props, slots);

        var html = dest.GetOutput();
        html.ShouldContain("<div class=\"inner\">");
        html.ShouldContain("<p>Slotted!</p>");
        html.ShouldContain("</slotted-widget>");
    }

    // ─── IClientComponent interface tests ─────────────────────────────────

    [Fact]
    public void ShouldImplementIClientComponent()
    {
        var island = new MyCounter();

        island.ShouldBeAssignableTo<IClientComponent>();
    }

    [Fact]
    public void ClientModuleUrlShouldReturnConfiguredUrl()
    {
        var island = new MyCounter();

        island.ClientModuleUrl.ShouldBe("/components/my-counter.js");
    }

    [Fact]
    public void ClientExportNameShouldDefaultToDefault()
    {
        var island = new MyCounter();

        island.ClientExportName.ShouldBe("default");
    }

    [Fact]
    public void ClientExportNameShouldReturnCustomExport()
    {
        var island = new AppHeader();

        island.ClientExportName.ShouldBe("AppHeader");
    }

    // ─── CreateMetadata tests ─────────────────────────────────

    [Fact]
    public void CreateMetadataShouldReturnMetadataForLoadDirective()
    {
        var island = new MyCounter();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.ComponentUrl.ShouldBe("/components/my-counter.js");
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Load);
        metadata.ComponentExport.ShouldBe("default");
        metadata.DisplayName.ShouldBe("my-counter");
    }

    [Fact]
    public void CreateMetadataShouldReturnMetadataForIdleDirective()
    {
        var island = new AppHeader();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
        metadata.ComponentExport.ShouldBe("AppHeader");
    }

    [Fact]
    public void CreateMetadataShouldReturnMetadataForVisibleDirective()
    {
        var island = new LazyChart();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
    }

    [Fact]
    public void CreateMetadataShouldReturnMetadataForMediaDirective()
    {
        var island = new DarkModeToggle();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Media);
        metadata.DirectiveValue.ShouldBe("(prefers-color-scheme: dark)");
    }

    [Fact]
    public void CreateMetadataShouldReturnNullWithoutDirective()
    {
        var island = new NoDirectiveWidget();

        var metadata = island.CreateMetadata();

        metadata.ShouldBeNull();
    }

    [Fact]
    public void CreateMetadataShouldUseTagNameAsDisplayName()
    {
        var island = new MyCounter();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DisplayName.ShouldBe("my-counter");
    }

    // ─── RenderIslandAsync tests ─────────────────────────────────

    [Fact]
    public async Task RenderIslandAsyncShouldWrapInIslandElement()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Count"] = 5 };

        var island = new MyCounter();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("</atoll-island>");
        html.ShouldContain("component-url=\"/components/my-counter.js\"");
        html.ShouldContain("client=\"load\"");
        html.ShouldContain("ssr");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldContainCustomElementInsideWrapper()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Count"] = 3 };

        var island = new MyCounter();
        await island.RenderIslandAsync(dest, props);

        var html = dest.GetOutput();
        html.ShouldContain("<my-counter");
        html.ShouldContain("</my-counter>");
        html.ShouldContain("<span class=\"count\">3</span>");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldWorkWithNoPropsOverload()
    {
        var dest = new StringRenderDestination();

        var island = new DarkModeToggle();
        await island.RenderIslandAsync(dest);

        var html = dest.GetOutput();
        html.ShouldContain("<atoll-island");
        html.ShouldContain("<dark-mode-toggle>");
        html.ShouldContain("<button>Toggle</button>");
        html.ShouldContain("client=\"media\"");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldHandleSlots()
    {
        var dest = new StringRenderDestination();
        var props = new Dictionary<string, object?>();
        var slots = new SlotBuilder()
            .Default(RenderFragment.FromHtml("<p>Slot content</p>"))
            .Build();

        var island = new SlottedWidget();
        await island.RenderIslandAsync(dest, props, slots);

        var html = dest.GetOutput();
        html.ShouldContain("<p>Slot content</p>");
        html.ShouldContain("</atoll-island>");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldThrowWithoutDirective()
    {
        var dest = new StringRenderDestination();

        var island = new NoDirectiveWidget();

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => island.RenderIslandAsync(dest));
        ex.Message.ShouldContain("does not have a client directive attribute");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldThrowForNullDestination()
    {
        var island = new MyCounter();

        await Should.ThrowAsync<ArgumentNullException>(
            () => island.RenderIslandAsync(null!, new Dictionary<string, object?>()));
    }

    [Fact]
    public async Task RenderIslandAsyncShouldThrowForNullProps()
    {
        var island = new MyCounter();

        await Should.ThrowAsync<ArgumentNullException>(
            () => island.RenderIslandAsync(new StringRenderDestination(), null!));
    }

    [Fact]
    public async Task RenderIslandAsyncShouldThrowForNullSlots()
    {
        var island = new MyCounter();

        await Should.ThrowAsync<ArgumentNullException>(
            () => island.RenderIslandAsync(
                new StringRenderDestination(),
                new Dictionary<string, object?>(),
                null!));
    }

    // ─── Integration: all directive types ─────────────────────────────────

    [Fact]
    public async Task ShouldRenderWithAllDirectiveTypes()
    {
        // Load
        var loadDest = new StringRenderDestination();
        await new MyCounter().RenderIslandAsync(loadDest);
        loadDest.GetOutput().ShouldContain("client=\"load\"");

        // Idle
        var idleDest = new StringRenderDestination();
        await new AppHeader().RenderIslandAsync(idleDest);
        idleDest.GetOutput().ShouldContain("client=\"idle\"");

        // Visible
        var visibleDest = new StringRenderDestination();
        await new LazyChart().RenderIslandAsync(visibleDest);
        visibleDest.GetOutput().ShouldContain("client=\"visible\"");

        // Media
        var mediaDest = new StringRenderDestination();
        await new DarkModeToggle().RenderIslandAsync(mediaDest);
        mediaDest.GetOutput().ShouldContain("client=\"media\"");
    }
}

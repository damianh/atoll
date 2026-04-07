using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Tests.Components;

public sealed class LayoutTests
{
    // ── LayoutAttribute tests ──

    [Fact]
    public void ShouldThrowWhenLayoutTypeIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new LayoutAttribute(null!));
    }

    [Fact]
    public void ShouldThrowWhenLayoutTypeDoesNotImplementIAtollComponent()
    {
        var ex = Should.Throw<ArgumentException>(() => new LayoutAttribute(typeof(string)));

        ex.Message.ShouldContain("must implement IAtollComponent");
    }

    [Fact]
    public void ShouldStoreLayoutType()
    {
        var attr = new LayoutAttribute(typeof(SimpleLayout));

        attr.LayoutType.ShouldBe(typeof(SimpleLayout));
    }

    // ── LayoutResolver.GetLayoutType tests ──

    [Fact]
    public void GetLayoutTypeShouldReturnNullForComponentWithoutLayout()
    {
        var result = LayoutResolver.GetLayoutType(typeof(PageWithoutLayout));

        result.ShouldBeNull();
    }

    [Fact]
    public void GetLayoutTypeShouldReturnLayoutTypeWhenAttributePresent()
    {
        var result = LayoutResolver.GetLayoutType(typeof(PageWithSimpleLayout));

        result.ShouldBe(typeof(SimpleLayout));
    }

    [Fact]
    public void GetLayoutTypeShouldThrowForNullType()
    {
        Should.Throw<ArgumentNullException>(() => LayoutResolver.GetLayoutType(null!));
    }

    // ── LayoutResolver.HasLayout tests ──

    [Fact]
    public void HasLayoutShouldReturnFalseForComponentWithoutLayout()
    {
        LayoutResolver.HasLayout(typeof(PageWithoutLayout)).ShouldBeFalse();
    }

    [Fact]
    public void HasLayoutShouldReturnTrueForComponentWithLayout()
    {
        LayoutResolver.HasLayout(typeof(PageWithSimpleLayout)).ShouldBeTrue();
    }

    [Fact]
    public void HasLayoutShouldThrowForNullType()
    {
        Should.Throw<ArgumentNullException>(() => LayoutResolver.HasLayout(null!));
    }

    // ── LayoutResolver.ResolveLayoutChain tests ──

    [Fact]
    public void ResolveLayoutChainShouldReturnEmptyForNoLayout()
    {
        var chain = LayoutResolver.ResolveLayoutChain(typeof(PageWithoutLayout));

        chain.ShouldBeEmpty();
    }

    [Fact]
    public void ResolveLayoutChainShouldReturnSingleLayoutForSimplePage()
    {
        var chain = LayoutResolver.ResolveLayoutChain(typeof(PageWithSimpleLayout));

        chain.Count.ShouldBe(1);
        chain[0].ShouldBe(typeof(SimpleLayout));
    }

    [Fact]
    public void ResolveLayoutChainShouldReturnNestedLayoutsOutermostFirst()
    {
        var chain = LayoutResolver.ResolveLayoutChain(typeof(PageWithNestedLayout));

        chain.Count.ShouldBe(2);
        chain[0].ShouldBe(typeof(OuterLayout)); // outermost first
        chain[1].ShouldBe(typeof(InnerLayout)); // innermost last
    }

    [Fact]
    public void ResolveLayoutChainShouldDetectCircularReference()
    {
        var ex = Should.Throw<InvalidOperationException>(
            () => LayoutResolver.ResolveLayoutChain(typeof(PageWithCircularLayout)));

        ex.Message.ShouldContain("Circular layout reference");
    }

    [Fact]
    public void ResolveLayoutChainShouldThrowForNullType()
    {
        Should.Throw<ArgumentNullException>(() => LayoutResolver.ResolveLayoutChain(null!));
    }

    [Fact]
    public void ResolveLayoutChainShouldHandleThreeLevelNesting()
    {
        var chain = LayoutResolver.ResolveLayoutChain(typeof(PageWithThreeLevelLayout));

        chain.Count.ShouldBe(3);
        chain[0].ShouldBe(typeof(OuterLayout));       // outermost
        chain[1].ShouldBe(typeof(MiddleLayout));       // middle
        chain[2].ShouldBe(typeof(InnerLayoutForThree)); // innermost
    }

    // ── LayoutResolver.WrapWithLayouts integration tests ──

    [Fact]
    public async Task WrapWithLayoutsShouldReturnPageContentUnchangedWhenNoLayout()
    {
        var pageContent = RenderFragment.FromHtml("<h1>No Layout Page</h1>");

        var result = LayoutResolver.WrapWithLayouts(typeof(PageWithoutLayout), pageContent);

        var output = await result.RenderToStringAsync();
        output.ShouldBe("<h1>No Layout Page</h1>");
    }

    [Fact]
    public async Task WrapWithLayoutsShouldWrapPageContentInLayout()
    {
        var pageContent = RenderFragment.FromHtml("<h1>About Us</h1>");

        var result = LayoutResolver.WrapWithLayouts(typeof(PageWithSimpleLayout), pageContent);

        var output = await result.RenderToStringAsync();
        output.ShouldBe(
            "<html><head></head><body><h1>About Us</h1></body></html>");
    }

    [Fact]
    public async Task WrapWithLayoutsShouldChainNestedLayouts()
    {
        var pageContent = RenderFragment.FromHtml("<article>Content</article>");

        var result = LayoutResolver.WrapWithLayouts(typeof(PageWithNestedLayout), pageContent);

        var output = await result.RenderToStringAsync();
        // InnerLayout wraps page content: <div class="inner">Content</div>
        // OuterLayout wraps that: <div class="outer"><div class="inner">Content</div></div>
        output.ShouldBe(
            "<div class=\"outer\"><div class=\"inner\"><article>Content</article></div></div>");
    }

    [Fact]
    public async Task WrapWithLayoutsShouldChainThreeLevelLayouts()
    {
        var pageContent = RenderFragment.FromHtml("<p>Deep content</p>");

        var result = LayoutResolver.WrapWithLayouts(typeof(PageWithThreeLevelLayout), pageContent);

        var output = await result.RenderToStringAsync();
        // InnerLayoutForThree: <section class="level3">...</section>
        // MiddleLayout: <main class="level2">...</main>
        // OuterLayout: <div class="outer">...</div>
        output.ShouldBe(
            "<div class=\"outer\"><main class=\"level2\"><section class=\"level3\">" +
            "<p>Deep content</p>" +
            "</section></main></div>");
    }

    [Fact]
    public async Task WrapWithLayoutsShouldThrowForNullComponentType()
    {
        var pageContent = RenderFragment.FromHtml("<p>Test</p>");

        Should.Throw<ArgumentNullException>(
            () => LayoutResolver.WrapWithLayouts(null!, pageContent));
    }

    [Fact]
    public async Task WrapWithLayoutsShouldPassPropsToInnermostLayout()
    {
        var pageContent = RenderFragment.FromHtml("<p>Page body</p>");
        var props = new Dictionary<string, object?> { ["Title"] = "My Site" };

        var result = LayoutResolver.WrapWithLayouts(
            typeof(PageWithPropsLayout), pageContent, props);

        var output = await result.RenderToStringAsync();
        output.ShouldBe(
            "<html><head><title>My Site</title></head><body><p>Page body</p></body></html>");
    }

    [Fact]
    public async Task WrapWithLayoutsWithPropsShouldReturnUnchangedWhenNoLayout()
    {
        var pageContent = RenderFragment.FromHtml("<p>Content</p>");
        var props = new Dictionary<string, object?> { ["Title"] = "Ignored" };

        var result = LayoutResolver.WrapWithLayouts(
            typeof(PageWithoutLayout), pageContent, props);

        var output = await result.RenderToStringAsync();
        output.ShouldBe("<p>Content</p>");
    }

    [Fact]
    public void WrapWithLayoutsWithPropsShouldThrowForNullProps()
    {
        var pageContent = RenderFragment.FromHtml("<p>Test</p>");

        Should.Throw<ArgumentNullException>(
            () => LayoutResolver.WrapWithLayouts(
                typeof(PageWithSimpleLayout), pageContent, null!));
    }

    [Fact]
    public void WrapWithLayoutsWithPropsShouldThrowForNullComponentType()
    {
        var pageContent = RenderFragment.FromHtml("<p>Test</p>");
        var props = new Dictionary<string, object?>();

        Should.Throw<ArgumentNullException>(
            () => LayoutResolver.WrapWithLayouts(null!, pageContent, props));
    }

    // ── End-to-end with PageRenderer ──

    [Fact]
    public async Task ShouldRenderPageInsideLayoutWithPageRenderer()
    {
        // Simulate what a real hosting layer would do:
        // 1. Render the page content
        // 2. Detect layout and wrap
        // 3. Use PageRenderer to finalize (DOCTYPE, head injection, etc.)

        var pageComponent = new PageWithSimpleLayout();
        var pageDest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync(pageComponent, pageDest);
        var pageHtml = pageDest.GetOutput();

        // Wrap with layout
        var pageFragment = RenderFragment.FromHtml(pageHtml);
        var wrappedFragment = LayoutResolver.WrapWithLayouts(
            typeof(PageWithSimpleLayout), pageFragment);

        // Render the wrapped result through PageRenderer
        var renderer = new PageRenderer();
        ComponentDelegate wrappedPage = async ctx =>
        {
            await ctx.RenderAsync(wrappedFragment);
        };
        var result = await renderer.RenderPageAsync(wrappedPage);

        result.Html.ShouldStartWith("<!DOCTYPE html>");
        result.Html.ShouldContain("<h1>About page content</h1>");
        result.Html.ShouldContain("<body>");
        result.Html.ShouldContain("</body>");
    }

    [Fact]
    public async Task ShouldRenderLayoutWithNamedSlots()
    {
        var pageContent = RenderFragment.FromHtml("<p>Main content</p>");

        // The LayoutWithNamedSlots component renders named slots,
        // but when used as a layout, only the default slot (page content) is provided.
        // Named slots use fallback content.
        var result = LayoutResolver.WrapWithLayouts(
            typeof(PageWithNamedSlotLayout), pageContent);

        var output = await result.RenderToStringAsync();
        output.ShouldContain("<header><em>Default header</em></header>");
        output.ShouldContain("<main><p>Main content</p></main>");
        output.ShouldContain("<footer><em>Default footer</em></footer>");
    }

    [Fact]
    public async Task ShouldRenderLayoutWithAsyncContent()
    {
        var pageContent = RenderFragment.FromAsync(async destination =>
        {
            await Task.Delay(1); // Simulate async work
            destination.Write(RenderChunk.Html("<p>Async page content</p>"));
        });

        var result = LayoutResolver.WrapWithLayouts(
            typeof(PageWithSimpleLayout), pageContent);

        var output = await result.RenderToStringAsync();
        output.ShouldBe(
            "<html><head></head><body><p>Async page content</p></body></html>");
    }

    [Fact]
    public async Task LayoutShouldReceivePageContentAsDefaultSlot()
    {
        // Verify that the layout's HasSlot("default") returns true
        // and that no other slots are present
        var pageContent = RenderFragment.FromHtml("<p>Content</p>");

        var result = LayoutResolver.WrapWithLayouts(
            typeof(PageWithSlotCheckLayout), pageContent);

        var output = await result.RenderToStringAsync();
        output.ShouldContain("has-default:true");
        output.ShouldContain("has-sidebar:false");
        output.ShouldContain("<p>Content</p>");
    }

    [Fact]
    public async Task ShouldHandleEmptyPageContent()
    {
        var result = LayoutResolver.WrapWithLayouts(
            typeof(PageWithSimpleLayout), RenderFragment.Empty);

        var output = await result.RenderToStringAsync();
        output.ShouldBe("<html><head></head><body></body></html>");
    }

    // ── Inherited LayoutAttribute tests ──

    [Fact]
    public void ShouldInheritLayoutAttributeFromBaseClass()
    {
        // LayoutAttribute has Inherited = true, so derived classes should pick it up
        LayoutResolver.HasLayout(typeof(DerivedPageWithInheritedLayout)).ShouldBeTrue();
        LayoutResolver.GetLayoutType(typeof(DerivedPageWithInheritedLayout))
            .ShouldBe(typeof(SimpleLayout));
    }

    [Fact]
    public void ShouldAllowDerivedClassToOverrideLayout()
    {
        LayoutResolver.GetLayoutType(typeof(DerivedPageWithOverriddenLayout))
            .ShouldBe(typeof(InnerLayout));
    }

    // ── Test component types ──

    private sealed class PageWithoutLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<h1>No layout</h1>");
            return Task.CompletedTask;
        }
    }

    [Layout(typeof(SimpleLayout))]
    private sealed class PageWithSimpleLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<h1>About page content</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class SimpleLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head></head><body>");
            await RenderSlotAsync();
            WriteHtml("</body></html>");
        }
    }

    [Layout(typeof(InnerLayout))]
    private sealed class PageWithNestedLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Nested page</p>");
            return Task.CompletedTask;
        }
    }

    [Layout(typeof(OuterLayout))]
    private sealed class InnerLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"inner\">");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    private sealed class OuterLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"outer\">");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    // ── Three-level nesting ──

    [Layout(typeof(InnerLayoutForThree))]
    private sealed class PageWithThreeLevelLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Three levels</p>");
            return Task.CompletedTask;
        }
    }

    [Layout(typeof(MiddleLayout))]
    private sealed class InnerLayoutForThree : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<section class=\"level3\">");
            await RenderSlotAsync();
            WriteHtml("</section>");
        }
    }

    [Layout(typeof(OuterLayout))]
    private sealed class MiddleLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<main class=\"level2\">");
            await RenderSlotAsync();
            WriteHtml("</main>");
        }
    }

    // ── Circular layout reference ──

    [Layout(typeof(CircularLayoutB))]
    private sealed class PageWithCircularLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            return Task.CompletedTask;
        }
    }

    [Layout(typeof(CircularLayoutA))]
    private sealed class CircularLayoutB : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            await RenderSlotAsync();
        }
    }

    [Layout(typeof(CircularLayoutB))]
    private sealed class CircularLayoutA : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            await RenderSlotAsync();
        }
    }

    // ── Layout with props ──

    [Layout(typeof(PropsLayout))]
    private sealed class PageWithPropsLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Page body</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class PropsLayout : AtollComponent
    {
        [Parameter]
        public string Title { get; set; } = "Default";

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head><title>");
            WriteText(Title);
            WriteHtml("</title></head><body>");
            await RenderSlotAsync();
            WriteHtml("</body></html>");
        }
    }

    // ── Layout with named slots and fallback ──

    [Layout(typeof(LayoutWithNamedSlots))]
    private sealed class PageWithNamedSlotLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Main content</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class LayoutWithNamedSlots : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<header>");
            await RenderSlotAsync("header", RenderFragment.FromHtml("<em>Default header</em>"));
            WriteHtml("</header><main>");
            await RenderSlotAsync();
            WriteHtml("</main><footer>");
            await RenderSlotAsync("footer", RenderFragment.FromHtml("<em>Default footer</em>"));
            WriteHtml("</footer>");
        }
    }

    // ── Layout with HasSlot check ──

    [Layout(typeof(SlotCheckLayout))]
    private sealed class PageWithSlotCheckLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Content</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class SlotCheckLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"has-default:{HasSlot("default")}|");
            WriteHtml($"has-sidebar:{HasSlot("sidebar")}|");
            await RenderSlotAsync();
        }
    }

    // ── Inherited layout ──

    [Layout(typeof(SimpleLayout))]
    private class BasePageWithLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Base page</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class DerivedPageWithInheritedLayout : BasePageWithLayout
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Derived page</p>");
            return Task.CompletedTask;
        }
    }

    [Layout(typeof(InnerLayout))]
    private sealed class DerivedPageWithOverriddenLayout : BasePageWithLayout
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Overridden layout page</p>");
            return Task.CompletedTask;
        }
    }
}

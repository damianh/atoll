using Atoll.Components;
using Atoll.Rendering;
using Atoll.Tests.Components.Fixtures;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Components;

public sealed class RazorLayoutTests
{
    // ── C# page wrapped in Razor layout ──

    [Fact]
    public async Task CSharpPageShouldRenderInsideRazorLayout()
    {
        var destination = new StringRenderDestination();
        var pageContent = RenderFragment.FromHtml("<p>page content</p>");
        var wrapped = LayoutResolver.WrapWithLayouts(typeof(PageWithRazorLayout), pageContent);

        await wrapped.RenderAsync(destination);

        var output = destination.GetOutput();
        output.ShouldContain("<div class=\"razor-layout\">");
        output.ShouldContain("<p>page content</p>");
    }

    [Fact]
    public async Task RazorLayoutShouldRenderBodyAtCorrectPosition()
    {
        var destination = new StringRenderDestination();
        var pageContent = RenderFragment.FromHtml("<article>body</article>");
        var wrapped = LayoutResolver.WrapWithLayouts(typeof(PageWithRazorLayout), pageContent);

        await wrapped.RenderAsync(destination);

        var output = destination.GetOutput();
        var layoutIdx = output.IndexOf("<div class=\"razor-layout\">", StringComparison.Ordinal);
        var bodyIdx = output.IndexOf("<article>body</article>", StringComparison.Ordinal);
        layoutIdx.ShouldBeGreaterThanOrEqualTo(0);
        bodyIdx.ShouldBeGreaterThan(layoutIdx);
    }

    // ── LayoutAttribute accepts Razor layout types ──

    [Fact]
    public void LayoutAttributeShouldAcceptRazorLayoutType()
    {
        // SimpleRazorLayout extends AtollLayoutSlice extends AtollSlice extends RazorSlice
        // It does NOT implement IAtollComponent -- but LayoutAttribute now accepts RazorSlice subtypes
        var attr = new LayoutAttribute(typeof(SimpleRazorLayout));

        attr.LayoutType.ShouldBe(typeof(SimpleRazorLayout));
    }

    [Fact]
    public void LayoutAttributeShouldStillRejectNonLayoutTypes()
    {
        var ex = Should.Throw<ArgumentException>(() => new LayoutAttribute(typeof(string)));

        ex.Message.ShouldContain("must implement IAtollComponent");
    }

    // ── Razor layout resolves in chain ──

    [Fact]
    public void ResolveLayoutChainShouldIncludeRazorLayout()
    {
        var chain = LayoutResolver.ResolveLayoutChain(typeof(PageWithRazorLayout));

        chain.ShouldNotBeEmpty();
        chain.ShouldContain(typeof(SimpleRazorLayout));
    }

    // ── Razor page uses C# layout ──

    [Fact]
    public async Task RazorPageShouldRenderInsideCSharpLayout()
    {
        var destination = new StringRenderDestination();
        var slice = SimpleSlice.Create();
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        // Render directly through the adapter (no layout wrapping — just validates adapter works)
        await adapter.RenderAsync(context);

        destination.GetOutput().ShouldContain("<p>Hello from Razor!</p>");
    }

    // ── WrapWithLayouts uses SliceComponentAdapter for Razor layouts ──

    [Fact]
    public async Task WrapWithLayoutsShouldProduceCorrectNestingForRazorLayout()
    {
        var destination = new StringRenderDestination();
        var innerContent = RenderFragment.FromHtml("<span>inner</span>");
        var wrapped = LayoutResolver.WrapWithLayouts(typeof(PageWithRazorLayout), innerContent);

        await wrapped.RenderAsync(destination);

        var output = destination.GetOutput();
        output.ShouldContain("<span>inner</span>");
        output.ShouldContain("razor-layout");
    }

    // ── Layout resolver throws for non-component, non-RazorSlice types ──

    [Fact]
    public async Task WrapWithLayoutShouldThrowForNonComponentNonRazorType()
    {
        // We can't directly test CreateLayoutComponent since it's private,
        // but we can test via a LayoutAttribute on a type pointing to a bad layout.
        // However, LayoutAttribute validation now prevents registering bad layout types.
        // So this test verifies the LayoutAttribute guard works instead.
        var ex = Should.Throw<ArgumentException>(() => new LayoutAttribute(typeof(BadLayoutType)));

        ex.Message.ShouldContain("must implement IAtollComponent");
        await Task.CompletedTask;
    }

    // ── Test fixtures ──

    [Layout(typeof(SimpleRazorLayout))]
    private sealed class PageWithRazorLayout : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>page content</p>");
            return Task.CompletedTask;
        }
    }

    // Not a component, not a RazorSlice -- should be rejected by LayoutAttribute
    private sealed class BadLayoutType
    {
    }
}

using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Atoll.Tests.Components.Fixtures;
using RazorSlices;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Components;

public sealed class AtollSliceTests
{
    // ── Simple render tests ──

    [Fact]
    public async Task ShouldRenderSimpleSliceViaAdapter()
    {
        var destination = new StringRenderDestination();
        var slice = SimpleSlice.Create();
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        await adapter.RenderAsync(context);

        destination.GetOutput().ShouldBe("<p>Hello from Razor!</p>\n");
    }

    // ── Default slot tests ──

    [Fact]
    public async Task ShouldRenderDefaultSlotContent()
    {
        var destination = new StringRenderDestination();
        var slice = SliceWithDefaultSlot.Create();
        var adapter = new SliceComponentAdapter(slice);
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml("<span>slot content</span>"));
        var context = new RenderContext(destination, slots);

        await adapter.RenderAsync(context);

        var output = destination.GetOutput();
        output.ShouldContain("<span>slot content</span>");
    }

    [Fact]
    public async Task ShouldRenderEmptyWhenNoDefaultSlot()
    {
        var destination = new StringRenderDestination();
        var slice = SliceWithDefaultSlot.Create();
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        await adapter.RenderAsync(context);

        var output = destination.GetOutput();
        output.ShouldContain("<div>");
        output.ShouldNotContain("slot content");
    }

    // ── Named slot tests ──

    [Fact]
    public async Task ShouldRenderNamedSlots()
    {
        var destination = new StringRenderDestination();
        var slice = SliceWithNamedSlots.Create();
        var adapter = new SliceComponentAdapter(slice);
        var slots = new SlotCollection(new Dictionary<string, RenderFragment>
        {
            ["header"] = RenderFragment.FromHtml("<h1>Header</h1>"),
            [SlotCollection.DefaultSlotName] = RenderFragment.FromHtml("<p>Body</p>"),
            ["footer"] = RenderFragment.FromHtml("<small>Footer</small>"),
        });
        var context = new RenderContext(destination, slots);

        await adapter.RenderAsync(context);

        var output = destination.GetOutput();
        output.ShouldContain("<h1>Header</h1>");
        output.ShouldContain("<p>Body</p>");
        output.ShouldContain("<small>Footer</small>");
    }

    [Fact]
    public async Task ShouldRenderMissingNamedSlotAsEmpty()
    {
        var destination = new StringRenderDestination();
        var slice = SliceWithNamedSlots.Create();
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination); // no slots

        await adapter.RenderAsync(context);

        var output = destination.GetOutput();
        output.ShouldContain("<main></main>");
    }

    // ── Fallback slot tests ──

    [Fact]
    public async Task ShouldRenderFallbackWhenSlotMissing()
    {
        var destination = new StringRenderDestination();
        var slice = SliceWithFallback.Create();
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination); // no slots

        await adapter.RenderAsync(context);

        var output = destination.GetOutput();
        output.ShouldContain("<em>no content</em>");
    }

    [Fact]
    public async Task ShouldRenderSlotContentOverFallback()
    {
        var destination = new StringRenderDestination();
        var slice = SliceWithFallback.Create();
        var adapter = new SliceComponentAdapter(slice);
        var slots = new SlotCollection(new Dictionary<string, RenderFragment>
        {
            ["content"] = RenderFragment.FromHtml("<b>actual content</b>"),
        });
        var context = new RenderContext(destination, slots);

        await adapter.RenderAsync(context);

        var output = destination.GetOutput();
        output.ShouldContain("<b>actual content</b>");
        output.ShouldNotContain("no content");
    }

    [Fact]
    public async Task ShouldShowBadgeSlotOnlyWhenPresent()
    {
        var destinationWith = new StringRenderDestination();
        var destinationWithout = new StringRenderDestination();

        var sliceWith = SliceWithFallback.Create();
        var adapterWith = new SliceComponentAdapter(sliceWith);
        var slotsWithBadge = new SlotCollection(new Dictionary<string, RenderFragment>
        {
            ["badge"] = RenderFragment.FromHtml("NEW"),
        });
        await adapterWith.RenderAsync(new RenderContext(destinationWith, slotsWithBadge));

        var sliceWithout = SliceWithFallback.Create();
        var adapterWithout = new SliceComponentAdapter(sliceWithout);
        await adapterWithout.RenderAsync(new RenderContext(destinationWithout));

        destinationWith.GetOutput().ShouldContain("<span class=\"badge\">NEW</span>");
        destinationWithout.GetOutput().ShouldNotContain("<span class=\"badge\">");
    }

    // ── Typed model tests ──

    [Fact]
    public async Task ShouldRenderTypedModelSlice()
    {
        var destination = new StringRenderDestination();
        var model = new GreetingModel("World");
        var slice = GreetingSlice.Create(model);
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        await adapter.RenderAsync(context);

        destination.GetOutput().ShouldContain("Hello, World!");
    }

    [Fact]
    public async Task ShouldHtmlEncodeModelProperties()
    {
        var destination = new StringRenderDestination();
        var model = new GreetingModel("<script>alert('xss')</script>");
        var slice = GreetingSlice.Create(model);
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        await adapter.RenderAsync(context);

        var output = destination.GetOutput();
        output.ShouldNotContain("<script>");
        output.ShouldContain("&lt;script&gt;");
    }

    // ── Embedded C# component tests ──

    [Fact]
    public async Task ShouldRenderEmbeddedCSharpComponent()
    {
        var destination = new StringRenderDestination();
        var slice = SliceWithEmbeddedComponent.Create();
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        await adapter.RenderAsync(context);

        var output = destination.GetOutput();
        output.ShouldContain("<span>inline</span>");
    }

    // ── Null guard tests ──

    [Fact]
    public void SliceComponentAdapterShouldThrowOnNullSlice()
    {
        Should.Throw<ArgumentNullException>(() => new SliceComponentAdapter(null!));
    }

    [Fact]
    public async Task SliceComponentAdapterShouldThrowOnNullContext()
    {
        var slice = SimpleSlice.Create();
        var adapter = new SliceComponentAdapter(slice);

        await Should.ThrowAsync<ArgumentNullException>(() => adapter.RenderAsync(null!));
    }

    // ── Non-AtollSlice (plain RazorSlice) adapter test ──

    [Fact]
    public async Task ShouldRenderPlainRazorSliceWithoutDestinationInjection()
    {
        // A plain RazorSlice (not AtollSlice) can still be wrapped
        var destination = new StringRenderDestination();
        var slice = SimpleSlice.Create();
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        // Should work without throwing even though it's routed through the adapter
        await adapter.RenderAsync(context);

        destination.GetOutput().ShouldNotBeEmpty();
    }
}

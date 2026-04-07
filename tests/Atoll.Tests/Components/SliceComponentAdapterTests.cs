using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Atoll.Tests.Components.Fixtures;

namespace Atoll.Tests.Components;

public sealed class SliceComponentAdapterTests
{
    // ── Adapter construction ──

    [Fact]
    public void ShouldThrowWhenSliceIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new SliceComponentAdapter(null!));
    }

    // ── RenderAsync context validation ──

    [Fact]
    public async Task ShouldThrowWhenContextIsNull()
    {
        var slice = SimpleSlice.Create();
        var adapter = new SliceComponentAdapter(slice);

        await Should.ThrowAsync<ArgumentNullException>(() => adapter.RenderAsync(null!));
    }

    // ── Rendering a plain slice ──

    [Fact]
    public async Task ShouldRenderSimpleSliceThroughAdapter()
    {
        var destination = new StringRenderDestination();
        var slice = SimpleSlice.Create();
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        await adapter.RenderAsync(context);

        destination.GetOutput().ShouldNotBeEmpty();
        destination.GetOutput().ShouldContain("Hello from Razor!");
    }

    // ── Slot injection via adapter ──

    [Fact]
    public async Task ShouldInjectSlotsIntoAtollSlice()
    {
        var destination = new StringRenderDestination();
        var slice = SliceWithDefaultSlot.Create();
        var adapter = new SliceComponentAdapter(slice);
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml("<b>injected</b>"));
        var context = new RenderContext(destination, slots);

        await adapter.RenderAsync(context);

        destination.GetOutput().ShouldContain("<b>injected</b>");
    }

    // ── Model (typed props) via slice creation ──

    [Fact]
    public async Task ShouldPassModelToTypedSlice()
    {
        var destination = new StringRenderDestination();
        var model = new GreetingModel("Alice");
        var slice = GreetingSlice.Create(model);
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        await adapter.RenderAsync(context);

        destination.GetOutput().ShouldContain("Hello, Alice!");
    }

    // ── Adapter implements IAtollComponent ──

    [Fact]
    public void AdapterShouldImplementIAtollComponent()
    {
        var slice = SimpleSlice.Create();
        var adapter = new SliceComponentAdapter(slice);

        adapter.ShouldBeAssignableTo<IAtollComponent>();
    }

    // ── Rendering via ComponentRenderer with instance ──

    [Fact]
    public async Task ShouldRenderViaComponentRendererInstance()
    {
        var destination = new StringRenderDestination();
        var slice = SimpleSlice.Create();
        var adapter = new SliceComponentAdapter(slice);

        await ComponentRenderer.RenderComponentAsync(adapter, destination);

        destination.GetOutput().ShouldContain("Hello from Razor!");
    }

    // ── Destination injected correctly via multiple renders ──

    [Fact]
    public async Task ShouldWriteToCorrectDestinationOnEachRender()
    {
        var dest1 = new StringRenderDestination();
        var dest2 = new StringRenderDestination();

        var model1 = new GreetingModel("Bob");
        var model2 = new GreetingModel("Carol");

        var slice1 = GreetingSlice.Create(model1);
        var slice2 = GreetingSlice.Create(model2);

        await new SliceComponentAdapter(slice1).RenderAsync(new RenderContext(dest1));
        await new SliceComponentAdapter(slice2).RenderAsync(new RenderContext(dest2));

        dest1.GetOutput().ShouldContain("Bob");
        dest2.GetOutput().ShouldContain("Carol");
        dest1.GetOutput().ShouldNotContain("Carol");
        dest2.GetOutput().ShouldNotContain("Bob");
    }

    // ── HTML encoding is handled by Razor ──

    [Fact]
    public async Task ShouldHtmlEncodeRazorExpressionOutput()
    {
        var destination = new StringRenderDestination();
        var model = new GreetingModel("O'Brien & Co.");
        var slice = GreetingSlice.Create(model);
        var adapter = new SliceComponentAdapter(slice);
        var context = new RenderContext(destination);

        await adapter.RenderAsync(context);

        var output = destination.GetOutput();
        // Razor encodes & as &amp; and ' as &#x27;
        output.ShouldNotContain("O'Brien & Co.");
        output.ShouldContain("O&#x27;Brien");
        output.ShouldContain("&amp;");
    }
}

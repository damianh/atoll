using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Slots;
using Atoll.Tests.Islands.Fixtures;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Islands;

public sealed class RazorIslandTests
{
    // ── AtollIslandSlice base class properties ──

    [Fact]
    public void ClientModuleUrlShouldReturnConfiguredUrl()
    {
        var slice = (AtollIslandSlice)CounterIslandSlice.Create();

        slice.ClientModuleUrl.ShouldBe("/scripts/counter-island.js");
    }

    [Fact]
    public void ClientExportNameShouldDefaultToDefault()
    {
        var slice = (AtollIslandSlice)CounterIslandSlice.Create();

        slice.ClientExportName.ShouldBe("default");
    }

    // ── CreateMetadata ──

    [Fact]
    public void CreateMetadataShouldReturnMetadataWhenDirectiveIsPresent()
    {
        var slice = (AtollIslandSlice)CounterIslandSlice.Create();

        var metadata = slice.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata!.ComponentUrl.ShouldBe("/scripts/counter-island.js");
        metadata.ComponentExport.ShouldBe("default");
    }

    [Fact]
    public void CreateMetadataShouldReturnNullWhenNoDirective()
    {
        // CounterIslandSlice has [ClientLoad] so it returns metadata.
        // We test the null path via a C# subclass (no attribute).
        var slice = new NoDirectiveIslandSlice();

        var metadata = slice.CreateMetadata();

        metadata.ShouldBeNull();
    }

    // ── RenderIslandAsync via instance method ──

    [Fact]
    public async Task RenderIslandAsyncShouldWrapOutputInAtollIslandTag()
    {
        var destination = new StringRenderDestination();
        var slice = (AtollIslandSlice)CounterIslandSlice.Create();

        await slice.RenderIslandAsync(destination);

        var output = destination.GetOutput();
        output.ShouldContain("<atoll-island");
        output.ShouldContain("</atoll-island>");
        output.ShouldContain("counter-island");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldIncludeComponentUrl()
    {
        var destination = new StringRenderDestination();
        var slice = (AtollIslandSlice)CounterIslandSlice.Create();

        await slice.RenderIslandAsync(destination);

        destination.GetOutput().ShouldContain("component-url");
        destination.GetOutput().ShouldContain("/scripts/counter-island.js");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldIncludeClientDirective()
    {
        var destination = new StringRenderDestination();
        var slice = (AtollIslandSlice)CounterIslandSlice.Create();

        await slice.RenderIslandAsync(destination);

        destination.GetOutput().ShouldContain("client=\"load\"");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldIncludeSsrContent()
    {
        var destination = new StringRenderDestination();
        var slice = (AtollIslandSlice)CounterIslandSlice.Create();

        await slice.RenderIslandAsync(destination);

        var output = destination.GetOutput();
        output.ShouldContain("Counter Island Content");
    }

    [Fact]
    public async Task RenderIslandAsyncShouldThrowWhenNoDirective()
    {
        var destination = new StringRenderDestination();
        var slice = new NoDirectiveIslandSlice();

        await Should.ThrowAsync<InvalidOperationException>(
            () => slice.RenderIslandAsync(destination));
    }

    // ── IslandRenderer.RenderSliceIslandAsync<TSlice> ──

    [Fact]
    public async Task RenderSliceIslandAsyncShouldWrapOutputInAtollIslandTag()
    {
        var destination = new StringRenderDestination();
        var slice = (AtollIslandSlice)CounterIslandSlice.Create();
        var metadata = slice.CreateMetadata()!;

        await IslandRenderer.RenderSliceIslandAsync<CounterIslandSlice>(
            destination, metadata, new Dictionary<string, object?>(), SlotCollection.Empty);

        var output = destination.GetOutput();
        output.ShouldContain("<atoll-island");
        output.ShouldContain("</atoll-island>");
        output.ShouldContain("Counter Island Content");
    }

    // ── IslandRenderer.RenderIslandAsync(Type) with RazorSlice type ──

    [Fact]
    public async Task TypeBasedRenderIslandAsyncShouldSupportAtollIslandSliceType()
    {
        var destination = new StringRenderDestination();
        var slice = (AtollIslandSlice)CounterIslandSlice.Create();
        var metadata = slice.CreateMetadata()!;

        // Pass the source-generated proxy type — it derives from AtollIslandSlice (→ RazorSlice)
        await IslandRenderer.RenderIslandAsync(
            destination, metadata, typeof(CounterIslandSlice),
            new Dictionary<string, object?>(), SlotCollection.Empty);

        var output = destination.GetOutput();
        output.ShouldContain("<atoll-island");
        output.ShouldContain("Counter Island Content");
    }

    // ── Test helper types ──

    private sealed class NoDirectiveIslandSlice : AtollIslandSlice
    {
        public override string ClientModuleUrl => "/scripts/no-directive.js";

        public override Task ExecuteAsync()
        {
            WriteLiteral("<div>No directive</div>");
            return Task.CompletedTask;
        }
    }
}

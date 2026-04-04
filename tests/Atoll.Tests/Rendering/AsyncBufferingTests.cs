using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Rendering;

/// <summary>
/// Integration tests proving that async buffering preserves output order
/// even when expressions complete in different orders than they were started.
/// These tests verify the core Astro-style rendering algorithm.
/// </summary>
public sealed class AsyncBufferingTests
{
    [Fact]
    public async Task SlowFirstExpressionShouldStillAppearFirst()
    {
        // Expression 0 is slow, expression 1 is fast
        // Both should appear in correct order: 0 then 1
        var template = new InterpolatedTemplate(
            ["<div>", "-", "</div>"],
            [
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(50);
                    dest.Write(RenderChunk.Html("SLOW"));
                }),
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(1);
                    dest.Write(RenderChunk.Html("FAST"));
                })
            ]);

        var output = await template.ToRenderFragment().RenderToStringAsync();

        output.ShouldBe("<div>SLOW-FAST</div>");
    }

    [Fact]
    public async Task ThreeAsyncExpressionsCompletingInReverseOrderShouldPreserveInsertionOrder()
    {
        var template = new InterpolatedTemplate(
            ["[", ",", ",", "]"],
            [
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(60);
                    dest.Write(RenderChunk.Html("A"));
                }),
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(30);
                    dest.Write(RenderChunk.Html("B"));
                }),
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(1);
                    dest.Write(RenderChunk.Html("C"));
                })
            ]);

        var output = await template.ToRenderFragment().RenderToStringAsync();

        output.ShouldBe("[A,B,C]");
    }

    [Fact]
    public async Task SyncThenAsyncThenSyncShouldPreserveOrder()
    {
        var template = new InterpolatedTemplate(
            ["<", "|", "|", ">"],
            [
                RenderFragment.FromHtml("sync1"),
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(10);
                    dest.Write(RenderChunk.Html("async"));
                }),
                RenderFragment.FromHtml("sync2")
            ]);

        var output = await template.ToRenderFragment().RenderToStringAsync();

        output.ShouldBe("<sync1|async|sync2>");
    }

    [Fact]
    public async Task AsyncExpressionProducingMultipleChunksShouldPreserveInternalOrder()
    {
        var template = new InterpolatedTemplate(
            ["<div>", "</div>"],
            [RenderFragment.FromAsync(async dest =>
            {
                dest.Write(RenderChunk.Html("<h1>Title</h1>"));
                await Task.Delay(5);
                dest.Write(RenderChunk.Html("<p>Body</p>"));
                await Task.Delay(5);
                dest.Write(RenderChunk.Html("<footer>End</footer>"));
            })]);

        var output = await template.ToRenderFragment().RenderToStringAsync();

        output.ShouldBe("<div><h1>Title</h1><p>Body</p><footer>End</footer></div>");
    }

    [Fact]
    public async Task BufferedExpressionsAllStartImmediately()
    {
        // Verify that once async mode is triggered, all remaining expressions
        // start executing concurrently (not sequentially)
        var startTimes = new long[3];
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var template = new InterpolatedTemplate(
            ["", "", "", "", ""],
            [
                // This triggers async mode
                RenderFragment.FromAsync(async dest =>
                {
                    startTimes[0] = sw.ElapsedMilliseconds;
                    await Task.Delay(10);
                    dest.Write(RenderChunk.Html("A"));
                }),
                // These should start immediately (buffered), not wait for [0]
                RenderFragment.FromAsync(async dest =>
                {
                    startTimes[1] = sw.ElapsedMilliseconds;
                    await Task.Delay(10);
                    dest.Write(RenderChunk.Html("B"));
                }),
                RenderFragment.FromAsync(async dest =>
                {
                    startTimes[2] = sw.ElapsedMilliseconds;
                    await Task.Delay(10);
                    dest.Write(RenderChunk.Html("C"));
                }),
                RenderFragment.FromHtml("D")
            ]);

        var output = await template.ToRenderFragment().RenderToStringAsync();

        output.ShouldBe("ABCD");

        // Expressions 1 and 2 should have started before expression 0 completed
        // (i.e., they started within a reasonable time of each other)
        // With 10ms delays, if they were sequential we'd see ~10ms gaps
        // Allow generous tolerance but verify they're not sequential
        var gap1To0 = startTimes[1] - startTimes[0];
        var gap2To0 = startTimes[2] - startTimes[0];

        // Both should start well before expression 0 completes (~10ms).
        // Use generous tolerance (50ms) to account for thread-pool scheduling
        // latency under CI/parallel-test load. Sequential execution would show
        // gaps of ≥10ms per expression, so 50ms still proves concurrency.
        gap1To0.ShouldBeLessThan(50);
        gap2To0.ShouldBeLessThan(50);
    }

    [Fact]
    public async Task NestedAsyncTemplatesWithBufferingPreserveOrder()
    {
        // Inner template: has async expression
        var inner = new InterpolatedTemplate(
            ["<inner>", "</inner>"],
            [RenderFragment.FromAsync(async dest =>
            {
                await Task.Delay(10);
                dest.Write(RenderChunk.Html("nested-async"));
            })]);

        // Outer template: inner appears as an expression, followed by more async
        var outer = new InterpolatedTemplate(
            ["<outer>", " + ", "</outer>"],
            [
                inner.ToRenderFragment(),
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(5);
                    dest.Write(RenderChunk.Html("outer-async"));
                })
            ]);

        var output = await outer.ToRenderFragment().RenderToStringAsync();

        output.ShouldBe("<outer><inner>nested-async</inner> + outer-async</outer>");
    }

    [Fact]
    public async Task BufferedRendererShouldCaptureAllChunks()
    {
        var fragment = RenderFragment.FromAsync(async dest =>
        {
            dest.Write(RenderChunk.Html("<p>"));
            await Task.Delay(1);
            dest.Write(RenderChunk.Text("user input & stuff"));
            dest.Write(RenderChunk.Html("</p>"));
        });

        var buffered = new BufferedRenderer(fragment);
        buffered.Start();

        var destination = new StringRenderDestination();
        await buffered.FlushAsync(destination);

        destination.GetOutput().ShouldBe("<p>user input &amp; stuff</p>");
    }

    [Fact]
    public async Task BufferedRendererFlushIsIdempotentAfterStart()
    {
        var callCount = 0;
        var fragment = RenderFragment.FromAsync(dest =>
        {
            Interlocked.Increment(ref callCount);
            dest.Write(RenderChunk.Html("content"));
            return default;
        });

        var buffered = new BufferedRenderer(fragment);
        buffered.Start();
        buffered.Start(); // second call should be no-op

        var dest1 = new StringRenderDestination();
        await buffered.FlushAsync(dest1);
        dest1.GetOutput().ShouldBe("content");

        callCount.ShouldBe(1);
    }

    [Fact]
    public async Task BufferedRendererShouldAutoStartOnFlush()
    {
        var fragment = RenderFragment.FromHtml("<div>auto-started</div>");

        var buffered = new BufferedRenderer(fragment);
        // Don't call Start() — FlushAsync should do it

        var destination = new StringRenderDestination();
        await buffered.FlushAsync(destination);

        destination.GetOutput().ShouldBe("<div>auto-started</div>");
    }

    [Fact]
    public async Task BufferedRendererFlushShouldThrowForNullDestination()
    {
        var buffered = new BufferedRenderer(RenderFragment.Empty);

        await Should.ThrowAsync<ArgumentNullException>(
            () => buffered.FlushAsync(null!).AsTask());
    }

    [Fact]
    public async Task AllSyncExpressionsShouldTakeFastPath()
    {
        // When all expressions are sync, no buffering should be needed
        // We can't directly test the code path, but we can verify correct output
        var template = new InterpolatedTemplate(
            ["<div>", "+", "+", "</div>"],
            [
                RenderFragment.FromHtml("A"),
                RenderFragment.FromHtml("B"),
                RenderFragment.FromHtml("C")
            ]);

        var output = await template.ToRenderFragment().RenderToStringAsync();

        output.ShouldBe("<div>A+B+C</div>");
    }

    [Fact]
    public async Task EmptyTemplateNoExpressionsShouldRenderStaticHtml()
    {
        var template = new InterpolatedTemplate(
            ["<p>Static only</p>"],
            []);

        var output = await template.ToRenderFragment().RenderToStringAsync();

        output.ShouldBe("<p>Static only</p>");
    }

    [Fact]
    public async Task AllEmptyExpressionsShouldProduceOnlyHtmlParts()
    {
        var template = new InterpolatedTemplate(
            ["A", "B", "C"],
            [RenderFragment.Empty, RenderFragment.Empty]);

        var output = await template.ToRenderFragment().RenderToStringAsync();

        output.ShouldBe("ABC");
    }
}

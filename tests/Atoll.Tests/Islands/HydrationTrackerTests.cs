using Atoll.Instructions;
using Atoll.Islands;

namespace Atoll.Tests.Islands;

public sealed class HydrationTrackerTests
{
    // ─── TryEmitBootstrap tests ─────────────────────────────────

    [Fact]
    public void TryEmitBootstrapShouldReturnTrueOnFirstCall()
    {
        var tracker = new HydrationTracker();

        tracker.TryEmitBootstrap().ShouldBeTrue();
    }

    [Fact]
    public void TryEmitBootstrapShouldReturnFalseOnSubsequentCalls()
    {
        var tracker = new HydrationTracker();
        tracker.TryEmitBootstrap();

        tracker.TryEmitBootstrap().ShouldBeFalse();
    }

    [Fact]
    public void HasBootstrapShouldBeFalseInitially()
    {
        var tracker = new HydrationTracker();

        tracker.HasBootstrap.ShouldBeFalse();
    }

    [Fact]
    public void HasBootstrapShouldBeTrueAfterEmit()
    {
        var tracker = new HydrationTracker();
        tracker.TryEmitBootstrap();

        tracker.HasBootstrap.ShouldBeTrue();
    }

    // ─── TryEmitDirective tests ─────────────────────────────────

    [Fact]
    public void TryEmitDirectiveShouldReturnTrueOnFirstCallPerType()
    {
        var tracker = new HydrationTracker();

        tracker.TryEmitDirective(ClientDirectiveType.Load).ShouldBeTrue();
        tracker.TryEmitDirective(ClientDirectiveType.Idle).ShouldBeTrue();
        tracker.TryEmitDirective(ClientDirectiveType.Visible).ShouldBeTrue();
        tracker.TryEmitDirective(ClientDirectiveType.Media).ShouldBeTrue();
    }

    [Fact]
    public void TryEmitDirectiveShouldReturnFalseOnDuplicateCall()
    {
        var tracker = new HydrationTracker();
        tracker.TryEmitDirective(ClientDirectiveType.Load);

        tracker.TryEmitDirective(ClientDirectiveType.Load).ShouldBeFalse();
    }

    [Fact]
    public void DifferentDirectiveTypesShouldTrackIndependently()
    {
        var tracker = new HydrationTracker();
        tracker.TryEmitDirective(ClientDirectiveType.Load);

        // Different directive type should still be new
        tracker.TryEmitDirective(ClientDirectiveType.Idle).ShouldBeTrue();
    }

    [Fact]
    public void HasDirectiveShouldBeFalseInitially()
    {
        var tracker = new HydrationTracker();

        tracker.HasDirective(ClientDirectiveType.Load).ShouldBeFalse();
    }

    [Fact]
    public void HasDirectiveShouldBeTrueAfterEmit()
    {
        var tracker = new HydrationTracker();
        tracker.TryEmitDirective(ClientDirectiveType.Visible);

        tracker.HasDirective(ClientDirectiveType.Visible).ShouldBeTrue();
        tracker.HasDirective(ClientDirectiveType.Load).ShouldBeFalse();
    }

    // ─── EmittedCount tests ─────────────────────────────────

    [Fact]
    public void EmittedCountShouldBeZeroInitially()
    {
        var tracker = new HydrationTracker();

        tracker.EmittedCount.ShouldBe(0);
    }

    [Fact]
    public void EmittedCountShouldIncrementForNewEntries()
    {
        var tracker = new HydrationTracker();
        tracker.TryEmitBootstrap();
        tracker.TryEmitDirective(ClientDirectiveType.Load);

        tracker.EmittedCount.ShouldBe(2);
    }

    [Fact]
    public void EmittedCountShouldNotIncrementForDuplicates()
    {
        var tracker = new HydrationTracker();
        tracker.TryEmitBootstrap();
        tracker.TryEmitBootstrap();
        tracker.TryEmitDirective(ClientDirectiveType.Load);
        tracker.TryEmitDirective(ClientDirectiveType.Load);

        tracker.EmittedCount.ShouldBe(2);
    }

    // ─── GetRequiredScripts tests ─────────────────────────────────

    [Fact]
    public void GetRequiredScriptsShouldReturnBothOnFirstCall()
    {
        var tracker = new HydrationTracker();

        var scripts = tracker.GetRequiredScripts(
            ClientDirectiveType.Load,
            "/scripts/atoll-island.js",
            "/scripts/atoll-directives.js");

        scripts.Count.ShouldBe(2);
    }

    [Fact]
    public void GetRequiredScriptsShouldReturnOnlyDirectiveOnSecondCallWithDifferentDirective()
    {
        var tracker = new HydrationTracker();
        tracker.GetRequiredScripts(
            ClientDirectiveType.Load,
            "/scripts/atoll-island.js",
            "/scripts/load.js");

        var scripts = tracker.GetRequiredScripts(
            ClientDirectiveType.Idle,
            "/scripts/atoll-island.js",
            "/scripts/idle.js");

        // Bootstrap already emitted, only directive is new
        scripts.Count.ShouldBe(1);
    }

    [Fact]
    public void GetRequiredScriptsShouldReturnEmptyForFullDuplicate()
    {
        var tracker = new HydrationTracker();
        tracker.GetRequiredScripts(
            ClientDirectiveType.Load,
            "/scripts/atoll-island.js",
            "/scripts/load.js");

        var scripts = tracker.GetRequiredScripts(
            ClientDirectiveType.Load,
            "/scripts/atoll-island.js",
            "/scripts/load.js");

        scripts.Count.ShouldBe(0);
    }

    [Fact]
    public void GetRequiredScriptsShouldProduceModuleScripts()
    {
        var tracker = new HydrationTracker();

        var scripts = tracker.GetRequiredScripts(
            ClientDirectiveType.Load,
            "/scripts/atoll-island.js",
            "/scripts/load.js");

        scripts.Count.ShouldBe(2);
        // Both should be module scripts (not inline)
        scripts[0].IsInline.ShouldBeFalse();
        scripts[1].IsInline.ShouldBeFalse();
    }

    [Fact]
    public void GetRequiredScriptsShouldThrowForNullIslandScriptUrl()
    {
        var tracker = new HydrationTracker();

        Should.Throw<ArgumentNullException>(
            () => tracker.GetRequiredScripts(ClientDirectiveType.Load, null!, "/scripts/load.js"));
    }

    [Fact]
    public void GetRequiredScriptsShouldThrowForNullDirectiveScriptUrl()
    {
        var tracker = new HydrationTracker();

        Should.Throw<ArgumentNullException>(
            () => tracker.GetRequiredScripts(ClientDirectiveType.Load, "/scripts/atoll-island.js", null!));
    }

    // ─── GetRequiredInlineScripts tests ─────────────────────────────────

    [Fact]
    public void GetRequiredInlineScriptsShouldReturnBothOnFirstCall()
    {
        var tracker = new HydrationTracker();

        var scripts = tracker.GetRequiredInlineScripts(ClientDirectiveType.Load);

        scripts.Count.ShouldBe(2);
        scripts[0].IsInline.ShouldBeTrue();
        scripts[1].IsInline.ShouldBeTrue();
    }

    [Fact]
    public void GetRequiredInlineScriptsShouldReturnOnlyDirectiveOnSecondCallWithDifferentDirective()
    {
        var tracker = new HydrationTracker();
        tracker.GetRequiredInlineScripts(ClientDirectiveType.Load);

        var scripts = tracker.GetRequiredInlineScripts(ClientDirectiveType.Idle);

        scripts.Count.ShouldBe(1);
    }

    [Fact]
    public void GetRequiredInlineScriptsShouldReturnEmptyForFullDuplicate()
    {
        var tracker = new HydrationTracker();
        tracker.GetRequiredInlineScripts(ClientDirectiveType.Load);

        var scripts = tracker.GetRequiredInlineScripts(ClientDirectiveType.Load);

        scripts.Count.ShouldBe(0);
    }

    // ─── AddToProcessor tests ─────────────────────────────────

    [Fact]
    public void AddToProcessorShouldAddScriptsToProcessor()
    {
        var tracker = new HydrationTracker();
        var processor = new InstructionProcessor();

        tracker.AddToProcessor(
            processor,
            ClientDirectiveType.Load,
            "/scripts/atoll-island.js",
            "/scripts/load.js");

        processor.Count.ShouldBe(2);
    }

    [Fact]
    public void AddToProcessorShouldNotDuplicateScripts()
    {
        var tracker = new HydrationTracker();
        var processor = new InstructionProcessor();

        tracker.AddToProcessor(
            processor,
            ClientDirectiveType.Load,
            "/scripts/atoll-island.js",
            "/scripts/load.js");

        tracker.AddToProcessor(
            processor,
            ClientDirectiveType.Load,
            "/scripts/atoll-island.js",
            "/scripts/load.js");

        processor.Count.ShouldBe(2);
    }

    [Fact]
    public void AddToProcessorShouldAddOnlyNewDirectiveForSecondIslandType()
    {
        var tracker = new HydrationTracker();
        var processor = new InstructionProcessor();

        tracker.AddToProcessor(
            processor,
            ClientDirectiveType.Load,
            "/scripts/atoll-island.js",
            "/scripts/load.js");

        tracker.AddToProcessor(
            processor,
            ClientDirectiveType.Idle,
            "/scripts/atoll-island.js",
            "/scripts/idle.js");

        // bootstrap + load + idle = 3
        processor.Count.ShouldBe(3);
    }

    [Fact]
    public void AddToProcessorShouldThrowForNullProcessor()
    {
        var tracker = new HydrationTracker();

        Should.Throw<ArgumentNullException>(
            () => tracker.AddToProcessor(
                null!,
                ClientDirectiveType.Load,
                "/scripts/atoll-island.js",
                "/scripts/load.js"));
    }

    // ─── Reset tests ─────────────────────────────────

    [Fact]
    public void ResetShouldClearAllTracking()
    {
        var tracker = new HydrationTracker();
        tracker.TryEmitBootstrap();
        tracker.TryEmitDirective(ClientDirectiveType.Load);
        tracker.TryEmitDirective(ClientDirectiveType.Idle);

        tracker.Reset();

        tracker.EmittedCount.ShouldBe(0);
        tracker.HasBootstrap.ShouldBeFalse();
        tracker.HasDirective(ClientDirectiveType.Load).ShouldBeFalse();
        tracker.HasDirective(ClientDirectiveType.Idle).ShouldBeFalse();
    }

    [Fact]
    public void ResetShouldAllowReEmitting()
    {
        var tracker = new HydrationTracker();
        tracker.TryEmitBootstrap();
        tracker.TryEmitDirective(ClientDirectiveType.Load);

        tracker.Reset();

        tracker.TryEmitBootstrap().ShouldBeTrue();
        tracker.TryEmitDirective(ClientDirectiveType.Load).ShouldBeTrue();
    }

    // ─── Deduplication scenario: multiple islands same type ─────────────────

    [Fact]
    public void FiveLoadIslandsShouldOnlyEmitOneBootstrapAndOneDirective()
    {
        var tracker = new HydrationTracker();
        var totalScripts = 0;

        for (var i = 0; i < 5; i++)
        {
            var scripts = tracker.GetRequiredScripts(
                ClientDirectiveType.Load,
                "/scripts/atoll-island.js",
                "/scripts/load.js");
            totalScripts += scripts.Count;
        }

        // First call: 2 scripts (bootstrap + load directive)
        // Calls 2-5: 0 scripts each
        totalScripts.ShouldBe(2);
        tracker.EmittedCount.ShouldBe(2);
    }

    [Fact]
    public void MixedDirectiveTypesShouldDeduplicateCorrectly()
    {
        var tracker = new HydrationTracker();
        var totalScripts = 0;

        // 3 load islands
        for (var i = 0; i < 3; i++)
        {
            totalScripts += tracker.GetRequiredScripts(
                ClientDirectiveType.Load, "/island.js", "/load.js").Count;
        }

        // 2 idle islands
        for (var i = 0; i < 2; i++)
        {
            totalScripts += tracker.GetRequiredScripts(
                ClientDirectiveType.Idle, "/island.js", "/idle.js").Count;
        }

        // 1 visible island
        totalScripts += tracker.GetRequiredScripts(
            ClientDirectiveType.Visible, "/island.js", "/visible.js").Count;

        // bootstrap(1) + load(1) + idle(1) + visible(1) = 4
        totalScripts.ShouldBe(4);
        tracker.EmittedCount.ShouldBe(4);
    }

    [Fact]
    public void AllFourDirectiveTypesShouldTrackSeparately()
    {
        var tracker = new HydrationTracker();

        tracker.TryEmitDirective(ClientDirectiveType.Load).ShouldBeTrue();
        tracker.TryEmitDirective(ClientDirectiveType.Idle).ShouldBeTrue();
        tracker.TryEmitDirective(ClientDirectiveType.Visible).ShouldBeTrue();
        tracker.TryEmitDirective(ClientDirectiveType.Media).ShouldBeTrue();

        tracker.HasDirective(ClientDirectiveType.Load).ShouldBeTrue();
        tracker.HasDirective(ClientDirectiveType.Idle).ShouldBeTrue();
        tracker.HasDirective(ClientDirectiveType.Visible).ShouldBeTrue();
        tracker.HasDirective(ClientDirectiveType.Media).ShouldBeTrue();
    }
}

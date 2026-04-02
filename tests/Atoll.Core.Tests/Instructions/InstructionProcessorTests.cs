using Atoll.Core.Instructions;
using Atoll.Core.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Instructions;

public sealed class InstructionProcessorTests
{
    // ── Add + deduplication ──

    [Fact]
    public void AddShouldReturnTrueForNewInstruction()
    {
        var processor = new InstructionProcessor();
        var instruction = HeadInstruction.Stylesheet("/css/main.css");

        processor.Add(instruction).ShouldBeTrue();
    }

    [Fact]
    public void AddShouldReturnFalseForDuplicateKey()
    {
        var processor = new InstructionProcessor();
        var first = HeadInstruction.Stylesheet("/css/main.css");
        var second = HeadInstruction.Stylesheet("/css/main.css");

        processor.Add(first).ShouldBeTrue();
        processor.Add(second).ShouldBeFalse();
    }

    [Fact]
    public void DuplicatesShouldNotIncrementCount()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/main.css"));
        processor.Add(HeadInstruction.Stylesheet("/css/main.css"));
        processor.Add(HeadInstruction.Stylesheet("/css/other.css"));

        processor.Count.ShouldBe(2);
    }

    [Fact]
    public void DifferentKeysShouldAllBeAdded()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/main.css"));
        processor.Add(HeadInstruction.Stylesheet("/css/other.css"));
        processor.Add(ScriptInstruction.External("/js/app.js"));

        processor.Count.ShouldBe(3);
    }

    // ── HasInstruction ──

    [Fact]
    public void HasInstructionShouldReturnFalseForMissingKey()
    {
        var processor = new InstructionProcessor();

        processor.HasInstruction("link:stylesheet:/css/main.css").ShouldBeFalse();
    }

    [Fact]
    public void HasInstructionShouldReturnTrueForAddedKey()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/main.css"));

        processor.HasInstruction("link:stylesheet:/css/main.css").ShouldBeTrue();
    }

    // ── GetInstructions ──

    [Fact]
    public void GetInstructionsShouldReturnAllInInsertionOrder()
    {
        var processor = new InstructionProcessor();
        var css = HeadInstruction.Stylesheet("/css/main.css");
        var script = ScriptInstruction.External("/js/app.js");
        var meta = HeadInstruction.Meta("description", "A test page");

        processor.Add(css);
        processor.Add(script);
        processor.Add(meta);

        var all = processor.GetInstructions();
        all.Count.ShouldBe(3);
        all[0].ShouldBeSameAs(css);
        all[1].ShouldBeSameAs(script);
        all[2].ShouldBeSameAs(meta);
    }

    [Fact]
    public void GetInstructionsGenericShouldFilterByType()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/main.css"));
        processor.Add(ScriptInstruction.External("/js/app.js"));
        processor.Add(HeadInstruction.Meta("description", "A test page"));
        processor.Add(ScriptInstruction.Module("/js/module.js"));

        var headInstructions = processor.GetInstructions<HeadInstruction>().ToList();
        var scriptInstructions = processor.GetInstructions<ScriptInstruction>().ToList();

        headInstructions.Count.ShouldBe(2);
        scriptInstructions.Count.ShouldBe(2);
    }

    // ── RenderAllAsync ──

    [Fact]
    public async Task RenderAllAsyncShouldRenderAllInstructions()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/main.css"));
        processor.Add(HeadInstruction.Meta("description", "Test page"));

        var dest = new StringRenderDestination();
        await processor.RenderAllAsync(dest);

        var output = dest.GetOutput();
        output.ShouldContain("<link rel=\"stylesheet\" href=\"/css/main.css\">");
        output.ShouldContain("<meta name=\"description\" content=\"Test page\">");
    }

    [Fact]
    public async Task RenderAllAsyncGenericShouldRenderOnlyMatchingType()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/main.css"));
        processor.Add(ScriptInstruction.External("/js/app.js"));
        processor.Add(HeadInstruction.Meta("description", "Test page"));

        var dest = new StringRenderDestination();
        await processor.RenderAllAsync<HeadInstruction>(dest);

        var output = dest.GetOutput();
        output.ShouldContain("<link rel=\"stylesheet\" href=\"/css/main.css\">");
        output.ShouldContain("<meta name=\"description\" content=\"Test page\">");
        output.ShouldNotContain("<script");
    }

    [Fact]
    public async Task RenderAllAsyncShouldSeparateWithNewlines()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/a.css"));
        processor.Add(HeadInstruction.Stylesheet("/css/b.css"));

        var dest = new StringRenderDestination();
        await processor.RenderAllAsync(dest);

        var output = dest.GetOutput();
        output.ShouldBe(
            "<link rel=\"stylesheet\" href=\"/css/a.css\">\n" +
            "<link rel=\"stylesheet\" href=\"/css/b.css\">\n");
    }

    // ── Clear ──

    [Fact]
    public void ClearShouldRemoveAllInstructions()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/main.css"));
        processor.Add(ScriptInstruction.External("/js/app.js"));

        processor.Clear();

        processor.Count.ShouldBe(0);
        processor.HasInstruction("link:stylesheet:/css/main.css").ShouldBeFalse();
    }

    [Fact]
    public void ClearShouldAllowReAddingPreviousKeys()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/main.css"));

        processor.Clear();
        processor.Add(HeadInstruction.Stylesheet("/css/main.css")).ShouldBeTrue();

        processor.Count.ShouldBe(1);
    }

    // ── Null argument validation ──

    [Fact]
    public void AddShouldThrowForNullInstruction()
    {
        var processor = new InstructionProcessor();

        Should.Throw<ArgumentNullException>(() => processor.Add(null!));
    }

    [Fact]
    public void HasInstructionShouldThrowForNullKey()
    {
        var processor = new InstructionProcessor();

        Should.Throw<ArgumentNullException>(() => processor.HasInstruction(null!));
    }

    [Fact]
    public async Task RenderAllAsyncShouldThrowForNullDestination()
    {
        var processor = new InstructionProcessor();

        await Should.ThrowAsync<ArgumentNullException>(
            () => processor.RenderAllAsync(null!).AsTask());
    }

    [Fact]
    public async Task RenderAllAsyncGenericShouldThrowForNullDestination()
    {
        var processor = new InstructionProcessor();

        await Should.ThrowAsync<ArgumentNullException>(
            () => processor.RenderAllAsync<HeadInstruction>(null!).AsTask());
    }
}

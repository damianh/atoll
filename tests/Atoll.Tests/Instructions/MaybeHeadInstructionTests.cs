using Atoll.Instructions;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Instructions;

public sealed class MaybeHeadInstructionTests
{
    // ── Rendering ──

    [Fact]
    public async Task RenderAsyncShouldDelegateToWrappedInstruction()
    {
        var head = HeadInstruction.Stylesheet("/css/main.css");
        var maybe = new MaybeHeadInstruction(head);
        var dest = new StringRenderDestination();

        await maybe.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<link rel=\"stylesheet\" href=\"/css/main.css\">");
    }

    [Fact]
    public async Task RenderAsyncShouldRenderRegardlessOfPropagateFlag()
    {
        var head = HeadInstruction.Meta("description", "A test page");
        var maybe = new MaybeHeadInstruction(head);
        maybe.Propagate = false;
        var dest = new StringRenderDestination();

        await maybe.RenderAsync(dest);

        dest.GetOutput().ShouldBe(
            "<meta name=\"description\" content=\"A test page\">");
    }

    // ── Key ──

    [Fact]
    public void KeyShouldMatchWrappedInstructionKey()
    {
        var head = HeadInstruction.Stylesheet("/css/theme.css");
        var maybe = new MaybeHeadInstruction(head);

        maybe.Key.ShouldBe(head.Key);
    }

    // ── Properties ──

    [Fact]
    public void HeadInstructionShouldReturnWrappedInstance()
    {
        var head = HeadInstruction.Title("My Page");
        var maybe = new MaybeHeadInstruction(head);

        maybe.HeadInstruction.ShouldBeSameAs(head);
    }

    [Fact]
    public void PropagateShouldDefaultToFalse()
    {
        var head = HeadInstruction.Title("My Page");
        var maybe = new MaybeHeadInstruction(head);

        maybe.Propagate.ShouldBeFalse();
    }

    [Fact]
    public void PropagateShouldBeSettable()
    {
        var head = HeadInstruction.Title("My Page");
        var maybe = new MaybeHeadInstruction(head);

        maybe.Propagate = true;

        maybe.Propagate.ShouldBeTrue();
    }

    // ── Deduplication with InstructionProcessor ──

    [Fact]
    public void ProcessorShouldDeduplicateMaybeHeadWithSameKeyAsHead()
    {
        var processor = new InstructionProcessor();
        var head = HeadInstruction.Stylesheet("/css/main.css");
        var maybe = new MaybeHeadInstruction(
            HeadInstruction.Stylesheet("/css/main.css"));

        processor.Add(head).ShouldBeTrue();
        processor.Add(maybe).ShouldBeFalse();
    }

    [Fact]
    public void ProcessorShouldFilterMaybeHeadInstructionsByType()
    {
        var processor = new InstructionProcessor();
        processor.Add(HeadInstruction.Stylesheet("/css/a.css"));
        processor.Add(new MaybeHeadInstruction(
            HeadInstruction.Stylesheet("/css/b.css")));
        processor.Add(ScriptInstruction.External("/js/app.js"));

        var maybes = processor.GetInstructions<MaybeHeadInstruction>().ToList();

        maybes.Count.ShouldBe(1);
        maybes[0].HeadInstruction.Key.ShouldBe("link:stylesheet:/css/b.css");
    }

    // ── Null argument validation ──

    [Fact]
    public void ConstructorShouldThrowForNullHeadInstruction()
    {
        Should.Throw<ArgumentNullException>(
            () => new MaybeHeadInstruction(null!));
    }

    [Fact]
    public async Task RenderAsyncShouldThrowForNullDestination()
    {
        var maybe = new MaybeHeadInstruction(HeadInstruction.Title("Test"));

        await Should.ThrowAsync<ArgumentNullException>(
            () => maybe.RenderAsync(null!).AsTask());
    }
}

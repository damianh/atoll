using Atoll.Instructions;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Instructions;

public sealed class DirectiveInstructionTests
{
    // ── Rendering ──

    [Fact]
    public async Task RenderAsyncShouldEmitMarkerWithoutValue()
    {
        var instruction = new DirectiveInstruction(ClientDirectiveType.Load);
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<!--[atoll:directive:Load]-->");
    }

    [Fact]
    public async Task RenderAsyncShouldEmitMarkerWithValue()
    {
        var instruction = new DirectiveInstruction(
            ClientDirectiveType.Media, "(max-width: 768px)");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe(
            "<!--[atoll:directive:Media:(max-width: 768px)]-->");
    }

    [Theory]
    [InlineData(ClientDirectiveType.Load)]
    [InlineData(ClientDirectiveType.Idle)]
    [InlineData(ClientDirectiveType.Visible)]
    [InlineData(ClientDirectiveType.Media)]
    public async Task RenderAsyncShouldIncludeDirectiveTypeInMarker(
        ClientDirectiveType directiveType)
    {
        var instruction = new DirectiveInstruction(directiveType);
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldContain($"directive:{directiveType}");
    }

    // ── Key ──

    [Fact]
    public void KeyShouldIncludeDirectiveTypeAndEmptyValueWhenNoValue()
    {
        var instruction = new DirectiveInstruction(ClientDirectiveType.Idle);

        instruction.Key.ShouldBe("directive:Idle:");
    }

    [Fact]
    public void KeyShouldIncludeDirectiveTypeAndValue()
    {
        var instruction = new DirectiveInstruction(
            ClientDirectiveType.Media, "(min-width: 1024px)");

        instruction.Key.ShouldBe("directive:Media:(min-width: 1024px)");
    }

    [Fact]
    public void SameDirectiveTypeShouldProduceSameKey()
    {
        var a = new DirectiveInstruction(ClientDirectiveType.Visible);
        var b = new DirectiveInstruction(ClientDirectiveType.Visible);

        a.Key.ShouldBe(b.Key);
    }

    [Fact]
    public void DifferentDirectiveTypesShouldProduceDifferentKeys()
    {
        var load = new DirectiveInstruction(ClientDirectiveType.Load);
        var idle = new DirectiveInstruction(ClientDirectiveType.Idle);

        load.Key.ShouldNotBe(idle.Key);
    }

    [Fact]
    public void DifferentMediaQueriesShouldProduceDifferentKeys()
    {
        var a = new DirectiveInstruction(
            ClientDirectiveType.Media, "(max-width: 768px)");
        var b = new DirectiveInstruction(
            ClientDirectiveType.Media, "(min-width: 1024px)");

        a.Key.ShouldNotBe(b.Key);
    }

    // ── Properties ──

    [Fact]
    public void DirectiveTypeShouldBeSetCorrectly()
    {
        var instruction = new DirectiveInstruction(ClientDirectiveType.Visible);

        instruction.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
    }

    [Fact]
    public void ValueShouldBeNullWhenNotProvided()
    {
        var instruction = new DirectiveInstruction(ClientDirectiveType.Load);

        instruction.Value.ShouldBeNull();
    }

    [Fact]
    public void ValueShouldBeSetWhenProvided()
    {
        var instruction = new DirectiveInstruction(
            ClientDirectiveType.Media, "(max-width: 600px)");

        instruction.Value.ShouldBe("(max-width: 600px)");
    }

    // ── Null argument validation ──

    [Fact]
    public async Task RenderAsyncShouldThrowForNullDestination()
    {
        var instruction = new DirectiveInstruction(ClientDirectiveType.Load);

        await Should.ThrowAsync<ArgumentNullException>(
            () => instruction.RenderAsync(null!).AsTask());
    }
}

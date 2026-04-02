using Atoll.Core.Instructions;
using Atoll.Core.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Instructions;

public sealed class ScriptInstructionTests
{
    [Fact]
    public async Task ExternalShouldRenderScriptElement()
    {
        var instruction = ScriptInstruction.External("/js/app.js");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<script src=\"/js/app.js\"></script>");
    }

    [Fact]
    public async Task ExternalShouldEncodeSrc()
    {
        var instruction = ScriptInstruction.External("/js/file\"name.js");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<script src=\"/js/file&quot;name.js\"></script>");
    }

    [Fact]
    public void ExternalShouldHaveCorrectKey()
    {
        var instruction = ScriptInstruction.External("/js/app.js");

        instruction.Key.ShouldBe("script:src:/js/app.js");
    }

    [Fact]
    public void ExternalShouldNotBeInline()
    {
        var instruction = ScriptInstruction.External("/js/app.js");

        instruction.IsInline.ShouldBeFalse();
    }

    [Fact]
    public async Task ModuleShouldRenderModuleScriptElement()
    {
        var instruction = ScriptInstruction.Module("/js/module.js");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<script type=\"module\" src=\"/js/module.js\"></script>");
    }

    [Fact]
    public void ModuleShouldHaveCorrectKey()
    {
        var instruction = ScriptInstruction.Module("/js/module.js");

        instruction.Key.ShouldBe("script:module:/js/module.js");
    }

    [Fact]
    public async Task InlineShouldRenderInlineScript()
    {
        var instruction = ScriptInstruction.Inline("counter", "let count = 0;");
        var dest = new StringRenderDestination();

        await instruction.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<script>let count = 0;</script>");
    }

    [Fact]
    public void InlineShouldHaveCorrectKey()
    {
        var instruction = ScriptInstruction.Inline("counter", "let count = 0;");

        instruction.Key.ShouldBe("script:inline:counter");
    }

    [Fact]
    public void InlineShouldBeMarkedAsInline()
    {
        var instruction = ScriptInstruction.Inline("counter", "let count = 0;");

        instruction.IsInline.ShouldBeTrue();
    }

    // ── Null argument validation ──

    [Fact]
    public void ExternalShouldThrowForNullSrc()
    {
        Should.Throw<ArgumentNullException>(() => ScriptInstruction.External(null!));
    }

    [Fact]
    public void ModuleShouldThrowForNullSrc()
    {
        Should.Throw<ArgumentNullException>(() => ScriptInstruction.Module(null!));
    }

    [Fact]
    public void InlineShouldThrowForNullScopeId()
    {
        Should.Throw<ArgumentNullException>(
            () => ScriptInstruction.Inline(null!, "js"));
    }

    [Fact]
    public void InlineShouldThrowForNullJavascript()
    {
        Should.Throw<ArgumentNullException>(
            () => ScriptInstruction.Inline("scope", null!));
    }
}

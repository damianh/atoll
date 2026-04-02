using Atoll.Core.Components;
using Atoll.Core.Css;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Css;

public sealed class StyleScopingTests
{
    // ── Basic scoping ──

    [Fact]
    public void ShouldScopeSimpleSelector()
    {
        var css = ".card { color: blue; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain(":where(.atoll-12345678) .card{");
        result.ShouldContain("color: blue;");
    }

    [Fact]
    public void ShouldScopeMultipleSelectors()
    {
        var css = ".card, .panel { color: blue; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain(":where(.atoll-12345678) .card");
        result.ShouldContain(":where(.atoll-12345678) .panel");
        result.ShouldContain("color: blue;");
    }

    [Fact]
    public void ShouldScopeElementSelector()
    {
        var css = "h1 { font-size: 2rem; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain(":where(.atoll-12345678) h1{");
        result.ShouldContain("font-size: 2rem;");
    }

    [Fact]
    public void ShouldScopeComplexSelectors()
    {
        var css = ".card > .title { font-weight: bold; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain(":where(.atoll-12345678) .card > .title{");
        result.ShouldContain("font-weight: bold;");
    }

    [Fact]
    public void ShouldScopeMultipleRules()
    {
        var css = ".card { color: blue; } .panel { color: red; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain(":where(.atoll-12345678) .card{");
        result.ShouldContain(":where(.atoll-12345678) .panel{");
    }

    [Fact]
    public void ShouldScopeIdSelector()
    {
        var css = "#main { width: 100%; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain(":where(.atoll-12345678) #main{");
        result.ShouldContain("width: 100%;");
    }

    [Fact]
    public void ShouldScopePseudoClassSelector()
    {
        var css = "a:hover { color: red; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain(":where(.atoll-12345678) a:hover{");
        result.ShouldContain("color: red;");
    }

    // ── Global selectors (not scoped) ──

    [Fact]
    public void ShouldNotScopeRootSelector()
    {
        var css = ":root { --color-primary: blue; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain(":root{");
        result.ShouldNotContain(":where(");
    }

    [Fact]
    public void ShouldNotScopeHtmlSelector()
    {
        var css = "html { font-size: 16px; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain("html{");
        result.ShouldNotContain(":where(");
    }

    [Fact]
    public void ShouldNotScopeBodySelector()
    {
        var css = "body { margin: 0; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain("body{");
        result.ShouldNotContain(":where(");
    }

    // ── At-rules ──

    [Fact]
    public void ShouldScopeInsideMediaQuery()
    {
        var css = "@media (max-width: 768px) { .card { display: block; } }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain("@media (max-width: 768px)");
        result.ShouldContain(":where(.atoll-12345678) .card");
    }

    [Fact]
    public void ShouldNotScopeKeyframes()
    {
        var css = "@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain("@keyframes fadeIn");
        result.ShouldNotContain(":where(.atoll-12345678)");
    }

    [Fact]
    public void ShouldNotScopeFontFace()
    {
        var css = "@font-face { font-family: 'Custom'; src: url('/fonts/custom.woff2'); }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain("@font-face");
        result.ShouldNotContain(":where(.atoll-12345678)");
    }

    [Fact]
    public void ShouldNotScopeImport()
    {
        var css = "@import url('reset.css');";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain("@import");
        result.ShouldNotContain(":where(.atoll-12345678)");
    }

    [Fact]
    public void ShouldScopeInsideSupportsQuery()
    {
        var css = "@supports (display: grid) { .container { display: grid; } }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain("@supports (display: grid)");
        result.ShouldContain(":where(.atoll-12345678) .container");
    }

    // ── Edge cases ──

    [Fact]
    public void ShouldReturnEmptyForEmptyCss()
    {
        var result = StyleScoper.Scope("", "atoll-12345678");
        result.ShouldBe("");
    }

    [Fact]
    public void ShouldReturnCssForEmptyScopeHash()
    {
        var result = StyleScoper.Scope(".card { color: blue; }", "");
        result.ShouldBe(".card { color: blue; }");
    }

    [Fact]
    public void ShouldPreserveComments()
    {
        var css = "/* component styles */ .card { color: blue; }";
        var result = StyleScoper.Scope(css, "atoll-12345678");

        result.ShouldContain("/* component styles */");
        result.ShouldContain(":where(.atoll-12345678) .card");
    }

    [Fact]
    public void ShouldScopeWithComponentType()
    {
        var css = ".card { color: blue; }";
        var result = StyleScoper.Scope(css, typeof(StyleScopingTests));

        var expectedHash = ScopeHashGenerator.Generate(typeof(StyleScopingTests));
        result.ShouldContain($":where(.{expectedHash}) .card");
    }

    // ── ExtractAndScope ──

    [Fact]
    public void ExtractAndScopeShouldReturnEmptyForComponentWithoutStyles()
    {
        var result = StyleScoper.ExtractAndScope(typeof(UnstyledComponent));
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void ExtractAndScopeShouldScopeStylesAttribute()
    {
        var result = StyleScoper.ExtractAndScope(typeof(StyledComponent));

        var expectedHash = ScopeHashGenerator.Generate(typeof(StyledComponent));
        result.ShouldContain($":where(.{expectedHash})");
        result.ShouldContain(".card");
    }

    [Fact]
    public void ExtractAndScopeShouldNotScopeGlobalStyles()
    {
        var result = StyleScoper.ExtractAndScope(typeof(GlobalStyledComponent));

        result.ShouldNotContain(":where(");
        result.ShouldContain("body { margin: 0; }");
    }

    [Fact]
    public void ExtractAndScopeShouldCombineMultipleStylesAttributes()
    {
        var result = StyleScoper.ExtractAndScope(typeof(MultiStyleComponent));

        result.ShouldContain(".card");
        result.ShouldContain(".panel");
    }

    // ── HasStyles / IsGlobal ──

    [Fact]
    public void HasStylesShouldReturnTrueForStyledComponent()
    {
        StyleScoper.HasStyles(typeof(StyledComponent)).ShouldBeTrue();
    }

    [Fact]
    public void HasStylesShouldReturnFalseForUnstyledComponent()
    {
        StyleScoper.HasStyles(typeof(UnstyledComponent)).ShouldBeFalse();
    }

    [Fact]
    public void IsGlobalShouldReturnTrueForGlobalComponent()
    {
        StyleScoper.IsGlobal(typeof(GlobalStyledComponent)).ShouldBeTrue();
    }

    [Fact]
    public void IsGlobalShouldReturnFalseForScopedComponent()
    {
        StyleScoper.IsGlobal(typeof(StyledComponent)).ShouldBeFalse();
    }

    // ── Null argument validation ──

    [Fact]
    public void ScopeShouldThrowForNullCss()
    {
        Should.Throw<ArgumentNullException>(
            () => StyleScoper.Scope(null!, "atoll-12345678"));
    }

    [Fact]
    public void ScopeShouldThrowForNullHash()
    {
        Should.Throw<ArgumentNullException>(
            () => StyleScoper.Scope(".card { }", (string)null!));
    }

    [Fact]
    public void ScopeWithTypeShouldThrowForNullCss()
    {
        Should.Throw<ArgumentNullException>(
            () => StyleScoper.Scope(null!, typeof(StyleScopingTests)));
    }

    [Fact]
    public void ScopeWithTypeShouldThrowForNullType()
    {
        Should.Throw<ArgumentNullException>(
            () => StyleScoper.Scope(".card { }", (Type)null!));
    }

    [Fact]
    public void ExtractAndScopeShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => StyleScoper.ExtractAndScope(null!));
    }

    [Fact]
    public void HasStylesShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => StyleScoper.HasStyles(null!));
    }

    [Fact]
    public void IsGlobalShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => StyleScoper.IsGlobal(null!));
    }

    // ── Test component types ──

    private sealed class UnstyledComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div>No styles</div>");
            return Task.CompletedTask;
        }
    }

    [Styles(".card { color: blue; }")]
    private sealed class StyledComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div class=\"card\">Styled</div>");
            return Task.CompletedTask;
        }
    }

    [GlobalStyle]
    [Styles("body { margin: 0; }")]
    private sealed class GlobalStyledComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div>Global</div>");
            return Task.CompletedTask;
        }
    }

    [Styles(".card { color: blue; }")]
    [Styles(".panel { color: red; }")]
    private sealed class MultiStyleComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div>Multi</div>");
            return Task.CompletedTask;
        }
    }
}

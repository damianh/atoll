using Atoll.Core.Css;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Css;

public sealed class CssMinifierTests
{
    // ── Basic minification ──

    [Fact]
    public void ShouldMinifyWhitespace()
    {
        var css = ".card {\n    color: blue;\n    padding: 1rem;\n}";
        var result = CssMinifier.Minify(css);

        result.ShouldNotContain("\n");
        result.ShouldContain(".card");
        result.ShouldContain("padding:1rem");
    }

    [Fact]
    public void ShouldMinifyMultipleRules()
    {
        var css = ".card { color: blue; }\n.panel { color: red; }";
        var result = CssMinifier.Minify(css);

        result.ShouldContain(".card");
        result.ShouldContain(".panel");
        result.Length.ShouldBeLessThan(css.Length);
    }

    [Fact]
    public void ShouldRemoveComments()
    {
        var css = "/* This is a comment */ .card { color: blue; }";
        var result = CssMinifier.Minify(css);

        result.ShouldNotContain("/* This is a comment */");
        result.ShouldContain(".card");
    }

    [Fact]
    public void ShouldReturnEmptyForEmptyInput()
    {
        CssMinifier.Minify("").ShouldBe(string.Empty);
    }

    // ── MinifyWithDiagnostics ──

    [Fact]
    public void DiagnosticsShouldSucceedForValidCss()
    {
        var result = CssMinifier.MinifyWithDiagnostics(".card { color: blue; }");

        result.Success.ShouldBeTrue();
        result.Css.ShouldContain(".card");
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void DiagnosticsShouldReturnEmptyForEmptyInput()
    {
        var result = CssMinifier.MinifyWithDiagnostics("");

        result.Success.ShouldBeTrue();
        result.Css.ShouldBe(string.Empty);
    }

    [Fact]
    public void DiagnosticsShouldPreserveValidProperties()
    {
        var css = ".card { color: blue; font-size: 16px; margin: 0 auto; }";
        var result = CssMinifier.MinifyWithDiagnostics(css);

        result.Success.ShouldBeTrue();
        result.Css.ShouldContain("font-size:16px");
    }

    // ── Null argument validation ──

    [Fact]
    public void MinifyShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(() => CssMinifier.Minify(null!));
    }

    [Fact]
    public void MinifyWithDiagnosticsShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => CssMinifier.MinifyWithDiagnostics(null!));
    }
}

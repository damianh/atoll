using System.Reflection;
using Atoll.Css;
using Atoll.Tests.Components.Fixtures;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Css;

public sealed class RazorStyleScopingTests
{
    // ── [Styles] attribute is preserved on source-generated proxy type ──

    [Fact]
    public void StylesAttributeShouldBePresentOnRazorSliceProxyType()
    {
        // StyledSlice.cshtml has @attribute [Styles(".styled-slice { color: blue; }")]
        // Verify the source-generated proxy type has the attribute.
        var attrs = typeof(StyledSlice).GetCustomAttributes<StylesAttribute>(inherit: false);

        attrs.ShouldNotBeEmpty();
    }

    [Fact]
    public void StylesAttributeCssShouldMatchDeclaredCss()
    {
        var attrs = typeof(StyledSlice).GetCustomAttributes<StylesAttribute>(inherit: false).ToList();

        attrs.Count.ShouldBe(1);
        attrs[0].Css.ShouldBe(".styled-slice { color: blue; }");
    }

    // ── CssAggregator.Add works with Razor-generated types ──

    [Fact]
    public void CssAggregatorShouldAddStylesFromRazorSliceProxyType()
    {
        var aggregator = new CssAggregator();

        var added = aggregator.Add(typeof(StyledSlice));

        added.ShouldBeTrue();
        aggregator.Count.ShouldBe(1);
    }

    [Fact]
    public void CssAggregatorOutputShouldContainScopedCssFromRazorSlice()
    {
        var aggregator = new CssAggregator();
        aggregator.Add(typeof(StyledSlice));

        var css = aggregator.GetCombinedCss();

        css.ShouldNotBeNullOrEmpty();
        css.ShouldContain(".styled-slice");
        css.ShouldContain(":where(");
    }

    // ── StyleScoper.Scope works with CSS from Razor-declared [Styles] ──

    [Fact]
    public void StyleScopingShouldScopeCssFromRazorAttribute()
    {
        var attr = typeof(StyledSlice)
            .GetCustomAttributes<StylesAttribute>(inherit: false)
            .Single();

        var scoped = StyleScoper.Scope(attr.Css, "atoll-test1234");

        scoped.ShouldContain(":where(.atoll-test1234) .styled-slice");
    }
}

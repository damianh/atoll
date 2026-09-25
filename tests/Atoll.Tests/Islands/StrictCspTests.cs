using Atoll.Components;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Tests.Shared;

namespace Atoll.Tests.Islands;

/// <summary>
/// Verifies island hydration needs no inline script, so pages work under
/// a strict Content-Security-Policy (<c>script-src 'self'</c>).
/// </summary>
public sealed class StrictCspTests
{
    private sealed class PageWithIslands : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml(
                "<html><head></head><body>" +
                "<atoll-island client=\"load\" component-url=\"/scripts/a.js\" ssr></atoll-island>" +
                "<atoll-island client=\"visible\" component-url=\"/scripts/b.js\" ssr></atoll-island>" +
                "</body></html>");
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task PageWithIslandsShouldOnlyEmitExternalHydrationScripts()
    {
        var result = await new PageRenderer().RenderPageAsync<PageWithIslands>();

        result.Html.ShouldContain("<script type=\"module\" src=\"/_atoll/island.js\"></script>");
        result.Html.ShouldContain("<script type=\"module\" src=\"/_atoll/directives.js\"></script>");
        result.Html.ShouldBeStrictCspCompatible();
    }

    [Fact]
    public void GenerateBootstrapScriptShouldBeExternal()
    {
        HydrationScriptGenerator.GenerateBootstrapScript("/_atoll/island.js").ShouldBeStrictCspCompatible();
    }

    // ── CspAssertions self-tests ──

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<script type=\"module\">/* marker */</script>")]
    [InlineData("<SCRIPT type=\"text/javascript\">x()</SCRIPT>")]
    [InlineData("<button onclick=\"x()\">b</button>")]
    [InlineData("<select aria-label=\"L\" onchange=\"go(this)\"></select>")]
    [InlineData("<img src=\"a.png\" onerror='x()'>")]
    public void CspAssertionsShouldRejectInlineCode(string html)
    {
        Should.Throw<ShouldAssertException>(() => html.ShouldBeStrictCspCompatible());
    }

    [Theory]
    [InlineData("<script src=\"/a.js\"></script>")]
    [InlineData("<script type=\"module\" src=\"/a.js\"></script>")]
    [InlineData("<script type=\"application/json\" id=\"d\">{\"onclick\":\"x\"}</script>")]
    [InlineData("<script type=\"application/ld+json\">{}</script>")]
    [InlineData("<button data-atoll-copy data-online=\"1\">b</button>")]
    [InlineData("<p>Turn it on=off in text</p>")]
    public void CspAssertionsShouldAllowExternalScriptsAndDataBlocks(string html)
    {
        html.ShouldBeStrictCspCompatible();
    }
}

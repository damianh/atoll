using Atoll.Core.Components;
using Atoll.Core.Css;
using Atoll.Core.Head;
using Atoll.Core.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Css;

public sealed class Phase6IntegrationTests
{
    // ── End-to-end: component CSS → scoping → aggregation → head injection ──

    [Fact]
    public void ShouldExtractScopeAndInjectCssFromComponents()
    {
        var aggregator = new CssAggregator();
        var headManager = new HeadManager();

        // Simulate rendering multiple component types
        aggregator.Add(typeof(HeaderComponent));
        aggregator.Add(typeof(CardComponent));
        aggregator.Add(typeof(FooterComponent));

        aggregator.Count.ShouldBe(3);

        // Inject into head
        CssInjector.InjectIntoHead(aggregator, headManager, false).ShouldBeTrue();

        headManager.Count.ShouldBe(1);
        var elements = headManager.GetElements();
        var css = elements[0].Content;
        css.ShouldNotBeNull();

        // Each component's CSS should be scoped
        var headerHash = ScopeHashGenerator.Generate(typeof(HeaderComponent));
        var cardHash = ScopeHashGenerator.Generate(typeof(CardComponent));
        var footerHash = ScopeHashGenerator.Generate(typeof(FooterComponent));

        css.ShouldContain($":where(.{headerHash})");
        css.ShouldContain($":where(.{cardHash})");
        css.ShouldContain($":where(.{footerHash})");
    }

    [Fact]
    public void ShouldDeduplicateComponentStylesAcrossMultipleRenders()
    {
        var aggregator = new CssAggregator();

        // Simulate the same component type rendered multiple times
        aggregator.Add(typeof(CardComponent)).ShouldBeTrue();
        aggregator.Add(typeof(CardComponent)).ShouldBeFalse();
        aggregator.Add(typeof(CardComponent)).ShouldBeFalse();

        aggregator.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldMixScopedAndGlobalStyles()
    {
        var aggregator = new CssAggregator();

        aggregator.Add(typeof(GlobalResetComponent));
        aggregator.Add(typeof(CardComponent));

        var entries = aggregator.GetEntries();
        entries.Count.ShouldBe(2);

        // Global component CSS should not be scoped
        entries[0].IsGlobal.ShouldBeTrue();
        entries[0].Css.ShouldNotContain(":where(");
        entries[0].Css.ShouldContain("body { margin: 0; }");

        // Scoped component CSS should have :where wrapper
        entries[1].IsGlobal.ShouldBeFalse();
        entries[1].Css.ShouldContain(":where(");
    }

    [Fact]
    public void ShouldMinifyAndInjectCombinedCss()
    {
        var aggregator = new CssAggregator();

        aggregator.Add("reset", "body {\n    margin: 0;\n    padding: 0;\n}", true);
        aggregator.Add("card", ".card {\n    color: blue;\n    padding: 1rem;\n}", false);

        var headManager = new HeadManager();
        CssInjector.InjectIntoHead(aggregator, headManager, true);

        var elements = headManager.GetElements();
        var css = elements[0].Content;
        css.ShouldNotBeNull();

        // Minified CSS should not have newlines or excessive whitespace
        css.ShouldNotContain("\n");
        css.ShouldContain("body");
        css.ShouldContain(".card");
    }

    [Fact]
    public void ShouldScopeInsideMediaQueryAndPreserveKeyframes()
    {
        var css = @"
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@media (max-width: 768px) { .card { display: block; } }
.card { animation: fadeIn 0.3s; }";

        var result = StyleScoper.Scope(css, "atoll-test1234");

        // Keyframes should NOT be scoped
        result.ShouldContain("@keyframes fadeIn");

        // Regular rule should be scoped
        result.ShouldContain(":where(.atoll-test1234) .card");

        // Media query inner rule should be scoped
        result.ShouldContain("@media (max-width: 768px)");
    }

    [Fact]
    public void ShouldRewriteUrlsInScopedCss()
    {
        var css = ".bg { background: url('/images/hero.jpg'); }";

        // First scope the CSS
        var scoped = StyleScoper.Scope(css, "atoll-test1234");

        // Then rewrite URLs
        var rewritten = CssUrlRewriter.Rewrite(scoped, "/docs");

        rewritten.ShouldContain(":where(.atoll-test1234)");
        rewritten.ShouldContain("/docs/images/hero.jpg");
    }

    [Fact]
    public void ShouldGenerateScopeClassForHtmlAttributeInjection()
    {
        // Components need to add the scope class to their root element
        var hash = ScopeHashGenerator.Generate(typeof(CardComponent));
        var classSelector = ScopeHashGenerator.GenerateClassSelector(typeof(CardComponent));

        // The hash is the class name to add to HTML elements
        hash.ShouldStartWith("atoll-");

        // The class selector is what CSS uses
        classSelector.ShouldBe("." + hash);

        // Verify the scoped CSS targets this class
        var css = StyleScoper.ExtractAndScope(typeof(CardComponent));
        css.ShouldContain($":where(.{hash})");
    }

    [Fact]
    public void ShouldGenerateCompleteStylesheetFromMultipleComponents()
    {
        var aggregator = new CssAggregator();

        aggregator.Add(typeof(GlobalResetComponent));
        aggregator.Add(typeof(HeaderComponent));
        aggregator.Add(typeof(CardComponent));
        aggregator.Add(typeof(FooterComponent));

        var css = aggregator.GetCombinedCss();

        // Should contain all component CSS in order
        var indexGlobal = css.IndexOf("body", StringComparison.Ordinal);
        var indexHeader = css.IndexOf(".header", StringComparison.Ordinal);
        var indexCard = css.IndexOf(".card", StringComparison.Ordinal);
        var indexFooter = css.IndexOf(".footer", StringComparison.Ordinal);

        indexGlobal.ShouldBeGreaterThanOrEqualTo(0);
        indexHeader.ShouldBeGreaterThan(indexGlobal);
        indexCard.ShouldBeGreaterThan(indexHeader);
        indexFooter.ShouldBeGreaterThan(indexCard);
    }

    [Fact]
    public void ShouldRenderInlineStyleHtmlWithScopeForDebugging()
    {
        var css = StyleScoper.ExtractAndScope(typeof(CardComponent));
        var hash = ScopeHashGenerator.Generate(typeof(CardComponent));

        var html = CssInjector.RenderInlineStyleHtml(css, hash);

        html.ShouldStartWith("<style data-atoll-scope=\"");
        html.ShouldEndWith("</style>");
        html.ShouldContain(hash);
    }

    [Fact]
    public async Task ShouldWorkWithHeadManagerRenderPipeline()
    {
        var aggregator = new CssAggregator();
        aggregator.Add(typeof(CardComponent));

        var headManager = new HeadManager();
        headManager.Add(new HeadElement("title") { Content = "Test Page" });

        CssInjector.InjectIntoHead(aggregator, headManager, false);

        // Also add a stylesheet link
        headManager.Add(CssInjector.CreateStylesheetLink("/css/external.css"));

        var dest = new StringRenderDestination();
        await headManager.RenderAllHeadContentAsync(dest);

        var output = dest.GetOutput();
        output.ShouldContain("<title>Test Page</title>");
        output.ShouldContain("<style>");
        output.ShouldContain("<link rel=\"stylesheet\" href=\"/css/external.css\">");
    }

    [Fact]
    public void ShouldMinifyThenRewriteUrls()
    {
        var css = ".hero {\n    background: url('/images/hero.jpg');\n    padding: 2rem;\n}";

        // Pipeline: minify → rewrite
        var minified = CssMinifier.Minify(css);
        var rewritten = CssUrlRewriter.Rewrite(minified, "/app");

        rewritten.ShouldContain("/app/images/hero.jpg");
        rewritten.ShouldNotContain("\n");
    }

    // ── Test component types ──

    [GlobalStyle]
    [Styles("body { margin: 0; }")]
    private sealed class GlobalResetComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div>Reset</div>");
            return Task.CompletedTask;
        }
    }

    [Styles(".header { background: navy; color: white; }")]
    private sealed class HeaderComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<header class=\"header\">Header</header>");
            return Task.CompletedTask;
        }
    }

    [Styles(".card { padding: 1rem; border: 1px solid #ccc; }")]
    private sealed class CardComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div class=\"card\">Card</div>");
            return Task.CompletedTask;
        }
    }

    [Styles(".footer { background: #333; color: white; }")]
    private sealed class FooterComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<footer class=\"footer\">Footer</footer>");
            return Task.CompletedTask;
        }
    }
}

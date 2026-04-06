using Atoll.Build.Pipeline;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Pipeline;

public sealed class HtmlPostProcessorTests
{
    private const string BasicHtml = """
        <!DOCTYPE html>
        <html>
        <head><title>Test</title></head>
        <body><h1>Hello</h1></body>
        </html>
        """;

    [Fact]
    public void ProcessShouldInjectCssLinkInHead()
    {
        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            CssHref = "/_atoll/styles.a1b2c3d4.css",
        });

        var result = processor.Process(BasicHtml);

        result.ShouldContain("/_atoll/styles.a1b2c3d4.css");
        result.ShouldContain("rel=\"stylesheet\"");
        result.ShouldContain("<link");
    }

    [Fact]
    public void ProcessShouldInjectJsScriptInBody()
    {
        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            JsHref = "/_atoll/scripts.a1b2c3d4.js",
        });

        var result = processor.Process(BasicHtml);

        result.ShouldContain("/_atoll/scripts.a1b2c3d4.js");
        result.ShouldContain("<script");
        result.ShouldContain("defer");
    }

    [Fact]
    public void ProcessShouldAddDeferByDefault()
    {
        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            JsHref = "/script.js",
            JsDefer = true,
        });

        var result = processor.Process(BasicHtml);

        result.ShouldContain("defer");
    }

    [Fact]
    public void ProcessShouldNotAddDeferWhenDisabled()
    {
        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            JsHref = "/script.js",
            JsDefer = false,
        });

        var result = processor.Process(BasicHtml);

        result.ShouldContain("src=\"/script.js\"");
        // AngleSharp serialization: check that defer is not in the script tag
        var scriptIndex = result.IndexOf("<script", StringComparison.Ordinal);
        var scriptEndIndex = result.IndexOf("</script>", scriptIndex, StringComparison.Ordinal);
        var scriptTag = result[scriptIndex..scriptEndIndex];
        scriptTag.ShouldNotContain("defer");
    }

    [Fact]
    public void ProcessShouldAddTypeModuleWhenEnabled()
    {
        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            JsHref = "/script.js",
            JsModule = true,
        });

        var result = processor.Process(BasicHtml);

        result.ShouldContain("type=\"module\"");
    }

    [Fact]
    public void ProcessShouldAdjustBasePathOnLinks()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><link rel="stylesheet" href="/styles.css"></head>
            <body><a href="/about">About</a></body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/styles.css");
        result.ShouldContain("/docs/about");
    }

    [Fact]
    public void ProcessShouldAdjustBasePathOnScriptsAndImages()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body>
            <script src="/app.js"></script>
            <img src="/images/logo.png">
            </body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/app.js");
        result.ShouldContain("/docs/images/logo.png");
    }

    [Fact]
    public void ProcessShouldNotAdjustExternalUrls()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body>
            <a href="https://example.com">External</a>
            <a href="//cdn.example.com/lib.js">CDN</a>
            <a href="mailto:test@test.com">Email</a>
            </body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("https://example.com");
        result.ShouldContain("//cdn.example.com/lib.js");
        result.ShouldContain("mailto:test@test.com");
        result.ShouldNotContain("/docs/https://");
        result.ShouldNotContain("/docs//cdn");
    }

    [Fact]
    public void ProcessShouldNotDoublePrefixUrls()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body><a href="/docs/page">Already prefixed</a></body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/page");
        result.ShouldNotContain("/docs/docs/page");
    }

    [Fact]
    public void ProcessShouldRemoveInlineStylesWhenEnabled()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head>
            <title>Test</title>
            <style>.card { padding: 1rem; }</style>
            </head>
            <body><div class="card">Content</div></body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            RemoveInlineStyles = true,
        });

        var result = processor.Process(html);

        result.ShouldNotContain("<style>");
        result.ShouldNotContain("padding");
    }

    [Fact]
    public void ProcessShouldNotRemoveInlineStylesByDefault()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head>
            <title>Test</title>
            <style>.card { padding: 1rem; }</style>
            </head>
            <body><div class="card">Content</div></body>
            </html>
            """;

        var processor = new HtmlPostProcessor();

        var result = processor.Process(html);

        result.ShouldContain("<style>");
        result.ShouldContain("padding");
    }

    [Fact]
    public void ProcessShouldReturnOriginalHtmlWhenNoOptionsSet()
    {
        var processor = new HtmlPostProcessor();

        var result = processor.Process(BasicHtml);

        result.ShouldContain("<title>Test</title>");
        result.ShouldContain("<h1>Hello</h1>");
    }

    [Fact]
    public void ProcessShouldReturnEmptyStringForEmptyInput()
    {
        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            CssHref = "/styles.css",
        });

        var result = processor.Process("");

        result.ShouldBe("");
    }

    [Fact]
    public void ProcessShouldPreserveDoctype()
    {
        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            CssHref = "/styles.css",
        });

        var result = processor.Process(BasicHtml);

        result.ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public void ProcessShouldHandleCssAndJsAndBasePathTogether()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body>
            <a href="/about">About</a>
            <img src="/logo.png">
            </body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            CssHref = "/_atoll/styles.abc123.css",
            JsHref = "/_atoll/scripts.def456.js",
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/_atoll/styles.abc123.css");
        result.ShouldContain("/_atoll/scripts.def456.js");
        result.ShouldContain("/docs/about");
        result.ShouldContain("/docs/logo.png");
    }

    [Fact]
    public void ProcessShouldAdjustSrcsetAttributes()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body>
            <img srcset="/images/small.png 480w, /images/large.png 800w">
            </body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/images/small.png");
        result.ShouldContain("/docs/images/large.png");
    }

    [Fact]
    public void ProcessShouldAdjustFormActionUrls()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body>
            <form action="/submit"></form>
            </body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/submit");
    }

    [Fact]
    public void ProcessShouldNotAdjustRelativeUrls()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body><a href="page.html">Relative</a></body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("page.html");
        result.ShouldNotContain("/docs/page.html");
    }

    [Fact]
    public void ProcessShouldRemoveMultipleInlineStyles()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head>
            <title>Test</title>
            <style>.a { color: red; }</style>
            <style>.b { color: blue; }</style>
            </head>
            <body><div>Content</div></body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            RemoveInlineStyles = true,
        });

        var result = processor.Process(html);

        result.ShouldNotContain("<style>");
    }

    [Fact]
    public void ProcessShouldThrowOnNull()
    {
        var processor = new HtmlPostProcessor();
        Should.Throw<ArgumentNullException>(() => processor.Process(null!));
    }

    [Fact]
    public void HtmlPostProcessorOptionsShouldHaveSensibleDefaults()
    {
        var options = new HtmlPostProcessorOptions();

        options.CssHref.ShouldBe("");
        options.JsHref.ShouldBe("");
        options.JsDefer.ShouldBeTrue();
        options.JsModule.ShouldBeFalse();
        options.BasePath.ShouldBe("");
        options.RemoveInlineStyles.ShouldBeFalse();
    }

    [Fact]
    public void ProcessShouldAdjustIslandComponentUrl()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body>
            <atoll-island component-url="/scripts/my-island.js" client="load"></atoll-island>
            </body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/scripts/my-island.js");
        result.ShouldNotContain("component-url=\"/scripts/my-island.js\"");
    }

    [Fact]
    public void ProcessShouldAdjustIslandBeforeHydrationUrl()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body>
            <atoll-island component-url="/scripts/my-island.js" before-hydration-url="/scripts/pre-hydrate.js" client="load"></atoll-island>
            </body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/scripts/my-island.js");
        result.ShouldContain("/docs/scripts/pre-hydrate.js");
    }

    [Fact]
    public void ProcessShouldNotDoublePrefixIslandComponentUrl()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body>
            <atoll-island component-url="/docs/scripts/my-island.js" client="load"></atoll-island>
            </body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/scripts/my-island.js");
        result.ShouldNotContain("/docs/docs/scripts/my-island.js");
    }

    [Fact]
    public void ProcessShouldHandleBasePathWithoutLeadingSlash()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body><a href="/about">About</a></body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "docs",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/about");
    }

    [Fact]
    public void ProcessShouldHandleBasePathWithTrailingSlash()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head><title>Test</title></head>
            <body><a href="/about">About</a></body>
            </html>
            """;

        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            BasePath = "/docs/",
        });

        var result = processor.Process(html);

        result.ShouldContain("/docs/about");
    }

    [Fact]
    public void ProcessShouldInjectCssBeforeExistingLinks()
    {
        var processor = new HtmlPostProcessor(new HtmlPostProcessorOptions
        {
            CssHref = "/_atoll/styles.css",
            RemoveInlineStyles = true,
        });

        var html = """
            <!DOCTYPE html>
            <html>
            <head>
            <title>Test</title>
            <style>.card { padding: 1rem; }</style>
            </head>
            <body><div>Content</div></body>
            </html>
            """;

        var result = processor.Process(html);

        result.ShouldContain("/_atoll/styles.css");
        result.ShouldNotContain("<style>");
    }
}

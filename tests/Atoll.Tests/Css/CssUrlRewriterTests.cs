using Atoll.Css;

namespace Atoll.Tests.Css;

public sealed class CssUrlRewriterTests
{
    // ── Basic rewriting ──

    [Fact]
    public void ShouldRewriteAbsolutePathUrl()
    {
        var css = "body { background: url('/images/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url('/docs/images/bg.png')");
    }

    [Fact]
    public void ShouldRewriteDoubleQuotedUrl()
    {
        var css = "body { background: url(\"/images/bg.png\"); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url(\"/docs/images/bg.png\")");
    }

    [Fact]
    public void ShouldRewriteUnquotedUrl()
    {
        var css = "body { background: url(/images/bg.png); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url(/docs/images/bg.png)");
    }

    [Fact]
    public void ShouldRewriteMultipleUrls()
    {
        var css = ".a { background: url('/img/a.png'); } .b { background: url('/img/b.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "/base");

        result.ShouldContain("url('/base/img/a.png')");
        result.ShouldContain("url('/base/img/b.png')");
    }

    // ── URLs that should NOT be rewritten ──

    [Fact]
    public void ShouldNotRewriteHttpUrl()
    {
        var css = "body { background: url('http://example.com/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url('http://example.com/bg.png')");
    }

    [Fact]
    public void ShouldNotRewriteHttpsUrl()
    {
        var css = "body { background: url('https://example.com/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url('https://example.com/bg.png')");
    }

    [Fact]
    public void ShouldNotRewriteProtocolRelativeUrl()
    {
        var css = "body { background: url('//cdn.example.com/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url('//cdn.example.com/bg.png')");
    }

    [Fact]
    public void ShouldNotRewriteDataUrl()
    {
        var css = "body { background: url('data:image/png;base64,abc123'); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url('data:image/png;base64,abc123')");
    }

    [Fact]
    public void ShouldNotRewriteRelativeUrl()
    {
        var css = "body { background: url('images/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url('images/bg.png')");
    }

    [Fact]
    public void ShouldNotRewriteHashUrl()
    {
        var css = "body { background: url('#fragment'); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url('#fragment')");
    }

    [Fact]
    public void ShouldNotRewriteBlobUrl()
    {
        var css = "body { background: url('blob:http://example.com/abc'); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldContain("url('blob:http://example.com/abc')");
    }

    // ── Base path normalization ──

    [Fact]
    public void ShouldHandleBasePathWithoutLeadingSlash()
    {
        var css = "body { background: url('/images/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "docs");

        result.ShouldContain("url('/docs/images/bg.png')");
    }

    [Fact]
    public void ShouldHandleBasePathWithTrailingSlash()
    {
        var css = "body { background: url('/images/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "/docs/");

        result.ShouldContain("url('/docs/images/bg.png')");
    }

    [Fact]
    public void ShouldReturnOriginalForRootBasePath()
    {
        var css = "body { background: url('/images/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "/");

        result.ShouldBe(css);
    }

    [Fact]
    public void ShouldReturnOriginalForEmptyBasePath()
    {
        var css = "body { background: url('/images/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "");

        result.ShouldBe(css);
    }

    // ── Edge cases ──

    [Fact]
    public void ShouldReturnEmptyForEmptyCss()
    {
        CssUrlRewriter.Rewrite("", "/docs").ShouldBe("");
    }

    [Fact]
    public void ShouldReturnCssWhenNoUrls()
    {
        var css = ".card { color: blue; }";
        var result = CssUrlRewriter.Rewrite(css, "/docs");

        result.ShouldBe(css);
    }

    [Fact]
    public void ShouldHandleNestedBasePath()
    {
        var css = "body { background: url('/images/bg.png'); }";
        var result = CssUrlRewriter.Rewrite(css, "/app/v2");

        result.ShouldContain("url('/app/v2/images/bg.png')");
    }

    // ── Null argument validation ──

    [Fact]
    public void RewriteShouldThrowForNullCss()
    {
        Should.Throw<ArgumentNullException>(
            () => CssUrlRewriter.Rewrite(null!, "/docs"));
    }

    [Fact]
    public void RewriteShouldThrowForNullBasePath()
    {
        Should.Throw<ArgumentNullException>(
            () => CssUrlRewriter.Rewrite(".a { }", null!));
    }
}

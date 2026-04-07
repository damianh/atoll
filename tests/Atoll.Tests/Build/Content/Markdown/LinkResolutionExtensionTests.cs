using Atoll.Build.Content.Markdown;

namespace Atoll.Build.Tests.Content.Markdown;

public sealed class LinkResolutionExtensionTests
{
    [Fact]
    public void ShouldRewriteRelativeMdLink()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](./page.md)", options);

        result.Html.ShouldContain("href=\"/docs/page/\"");
    }

    [Fact]
    public void ShouldPreserveFragmentOnRewrittenLink()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](./page.md#anchor)", options);

        result.Html.ShouldContain("href=\"/docs/page/#anchor\"");
    }

    [Fact]
    public void ShouldRewriteMdxExtension()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](./sub/page.mdx)", options);

        result.Html.ShouldContain("href=\"/docs/sub/page/\"");
    }

    [Fact]
    public void ShouldRewriteMdaExtension()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](./sub/page.mda)", options);

        result.Html.ShouldContain("href=\"/docs/sub/page/\"");
    }

    [Fact]
    public void ShouldNotRewriteAbsoluteHttpsLink()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](https://example.com)", options);

        result.Html.ShouldContain("href=\"https://example.com\"");
    }

    [Fact]
    public void ShouldNotRewriteAbsoluteHttpLink()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](http://example.com)", options);

        result.Html.ShouldContain("href=\"http://example.com\"");
    }

    [Fact]
    public void ShouldNotRewriteRootRelativeLink()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](/absolute/path)", options);

        result.Html.ShouldContain("href=\"/absolute/path\"");
    }

    [Fact]
    public void ShouldRewriteRootRelativeMdLink()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](/docs/tokens/pop.md)", options);

        result.Html.ShouldContain("href=\"/docs/tokens/pop/\"");
    }

    [Fact]
    public void ShouldRewriteRootRelativeMdLinkWithFragment()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](/docs/tokens/pop.md#section)", options);

        result.Html.ShouldContain("href=\"/docs/tokens/pop/#section\"");
    }

    [Fact]
    public void ShouldRewriteRootRelativeMdxLink()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](/docs/tokens/pop.mdx)", options);

        result.Html.ShouldContain("href=\"/docs/tokens/pop/\"");
    }

    [Fact]
    public void ShouldNotRewriteRootRelativePdfLink()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](/downloads/file.pdf)", options);

        result.Html.ShouldContain("href=\"/downloads/file.pdf\"");
    }

    [Fact]
    public void ShouldNotPrependBasePathToRootRelativeLink()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/site" }
        };

        var result = MarkdownRenderer.Render("[link](/docs/page.md)", options);

        // Should NOT prepend /site — the link already has its full path.
        result.Html.ShouldContain("href=\"/docs/page/\"");
        result.Html.ShouldNotContain("href=\"/site/docs/page/\"");
    }

    [Fact]
    public void ShouldNotAddTrailingSlashWhenDisabled()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions
            {
                BasePath = "/docs",
                AddTrailingSlash = false
            }
        };

        var result = MarkdownRenderer.Render("[link](./page.md)", options);

        result.Html.ShouldContain("href=\"/docs/page\"");
        result.Html.ShouldNotContain("href=\"/docs/page/\"");
    }

    [Fact]
    public void ShouldWorkWithEmptyBasePath()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "" }
        };

        var result = MarkdownRenderer.Render("[link](./page.md)", options);

        result.Html.ShouldContain("href=\"/page/\"");
    }

    [Fact]
    public void ShouldNotRewriteLinkWithNoMdExtension()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](./page)", options);

        result.Html.ShouldContain("href=\"./page\"");
    }

    [Fact]
    public void ShouldNotModifyLinksWhenLinkResolutionIsNull()
    {
        var options = new MarkdownOptions { LinkResolution = null };

        var result = MarkdownRenderer.Render("[link](./page.md)", options);

        result.Html.ShouldContain("href=\"./page.md\"");
    }

    [Fact]
    public void ShouldHandleNestedPathWithSubdirectory()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("[link](./guides/getting-started.md)", options);

        result.Html.ShouldContain("href=\"/docs/guides/getting-started/\"");
    }

    [Fact]
    public void ExistingMarkdownRendererTestsShouldStillPass()
    {
        // Verify existing rendering still works with LinkResolution enabled.
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" }
        };

        var result = MarkdownRenderer.Render("**bold** and *italic*\n\n[Atoll](https://atoll.dev)", options);

        result.Html.ShouldContain("<strong>bold</strong>");
        result.Html.ShouldContain("<em>italic</em>");
        result.Html.ShouldContain("href=\"https://atoll.dev\""); // absolute link untouched
    }
}

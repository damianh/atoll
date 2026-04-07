using Atoll.Build.Content.Markdown;

namespace Atoll.Build.Tests.Content.Markdown;

public sealed class ExternalLinkExtensionTests
{
    [Fact]
    public void ShouldAddTargetAndRelToHttpsLink()
    {
        var options = new MarkdownOptions
        {
            ExternalLinks = new ExternalLinkOptions()
        };

        var result = MarkdownRenderer.Render("[link](https://example.com)", options);

        result.Html.ShouldContain("href=\"https://example.com\"");
        result.Html.ShouldContain("target=\"_blank\"");
        result.Html.ShouldContain("rel=\"noopener noreferrer\"");
    }

    [Fact]
    public void ShouldAddTargetAndRelToHttpLink()
    {
        var options = new MarkdownOptions
        {
            ExternalLinks = new ExternalLinkOptions()
        };

        var result = MarkdownRenderer.Render("[link](http://example.com)", options);

        result.Html.ShouldContain("href=\"http://example.com\"");
        result.Html.ShouldContain("target=\"_blank\"");
        result.Html.ShouldContain("rel=\"noopener noreferrer\"");
    }

    [Fact]
    public void ShouldNotModifyRelativeLink()
    {
        var options = new MarkdownOptions
        {
            ExternalLinks = new ExternalLinkOptions()
        };

        var result = MarkdownRenderer.Render("[link](./page)", options);

        result.Html.ShouldNotContain("target=");
        result.Html.ShouldNotContain("rel=");
    }

    [Fact]
    public void ShouldNotModifyRootRelativeLink()
    {
        var options = new MarkdownOptions
        {
            ExternalLinks = new ExternalLinkOptions()
        };

        var result = MarkdownRenderer.Render("[link](/about)", options);

        result.Html.ShouldNotContain("target=");
        result.Html.ShouldNotContain("rel=");
    }

    [Fact]
    public void ShouldSkipExcludedHost()
    {
        var options = new MarkdownOptions
        {
            ExternalLinks = new ExternalLinkOptions
            {
                ExcludedHosts = ["example.com"]
            }
        };

        var result = MarkdownRenderer.Render("[link](https://example.com)", options);

        result.Html.ShouldNotContain("target=");
        result.Html.ShouldNotContain("rel=");
    }

    [Fact]
    public void ShouldNotSkipNonExcludedHost()
    {
        var options = new MarkdownOptions
        {
            ExternalLinks = new ExternalLinkOptions
            {
                ExcludedHosts = ["example.com"]
            }
        };

        var result = MarkdownRenderer.Render("[link](https://other.com)", options);

        result.Html.ShouldContain("target=\"_blank\"");
    }

    [Fact]
    public void ShouldOmitTargetWhenSetToNull()
    {
        var options = new MarkdownOptions
        {
            ExternalLinks = new ExternalLinkOptions { Target = null }
        };

        var result = MarkdownRenderer.Render("[link](https://example.com)", options);

        result.Html.ShouldNotContain("target=");
        result.Html.ShouldContain("rel=\"noopener noreferrer\"");
    }

    [Fact]
    public void ShouldOmitRelWhenSetToNull()
    {
        var options = new MarkdownOptions
        {
            ExternalLinks = new ExternalLinkOptions { Rel = null }
        };

        var result = MarkdownRenderer.Render("[link](https://example.com)", options);

        result.Html.ShouldContain("target=\"_blank\"");
        result.Html.ShouldNotContain("rel=");
    }

    [Fact]
    public void ShouldNotModifyLinksWhenExternalLinksIsNull()
    {
        var options = new MarkdownOptions { ExternalLinks = null };

        var result = MarkdownRenderer.Render("[link](https://example.com)", options);

        result.Html.ShouldNotContain("target=");
        result.Html.ShouldNotContain("rel=");
    }

    [Fact]
    public void ShouldWorkWithBothExtensionsEnabled()
    {
        var options = new MarkdownOptions
        {
            LinkResolution = new LinkResolutionOptions { BasePath = "/docs" },
            ExternalLinks = new ExternalLinkOptions()
        };

        var result = MarkdownRenderer.Render("[internal](./page.md) and [external](https://example.com)", options);

        // Internal link is resolved to clean URL and has no external attributes.
        result.Html.ShouldContain("href=\"/docs/page/\"");
        // External link keeps its URL and gets security attributes.
        result.Html.ShouldContain("href=\"https://example.com\"");
        result.Html.ShouldContain("target=\"_blank\"");
        result.Html.ShouldContain("rel=\"noopener noreferrer\"");
        // The resolved internal link should NOT have target/_blank applied.
        result.Html.ShouldNotContain("href=\"/docs/page/\" target=");
    }

    [Fact]
    public void ShouldMatchExcludedHostCaseInsensitively()
    {
        var options = new MarkdownOptions
        {
            ExternalLinks = new ExternalLinkOptions
            {
                ExcludedHosts = ["Example.COM"]
            }
        };

        var result = MarkdownRenderer.Render("[link](https://example.com)", options);

        result.Html.ShouldNotContain("target=");
    }
}

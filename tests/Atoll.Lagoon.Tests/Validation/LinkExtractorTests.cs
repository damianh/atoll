using Atoll.Lagoon.Validation;

namespace Atoll.Lagoon.Tests.Validation;

public sealed class LinkExtractorTests
{
    [Fact]
    public void ShouldExtractInternalLinkFromHtml()
    {
        var html = "<a href=\"/docs/getting-started/\">Getting Started</a>";

        var links = LinkExtractor.Extract(html, "/");

        links.Count.ShouldBe(1);
        links[0].Href.ShouldBe("/docs/getting-started/");
        links[0].Kind.ShouldBe(LinkKind.Internal);
        links[0].Path.ShouldBe("/docs/getting-started/");
        links[0].Fragment.ShouldBeNull();
        links[0].IsInternal.ShouldBeTrue();
    }

    [Fact]
    public void ShouldExtractFragmentFromInternalLink()
    {
        var html = "<a href=\"/docs/page/#section-one\">Section One</a>";

        var links = LinkExtractor.Extract(html, "/");

        links.Count.ShouldBe(1);
        links[0].Href.ShouldBe("/docs/page/#section-one");
        links[0].Kind.ShouldBe(LinkKind.Internal);
        links[0].Path.ShouldBe("/docs/page/");
        links[0].Fragment.ShouldBe("section-one");
    }

    [Fact]
    public void ShouldClassifyExternalLinksAsExternal()
    {
        var html = "<a href=\"https://example.com\">External</a>";

        var links = LinkExtractor.Extract(html, "/");

        links.Count.ShouldBe(1);
        links[0].Kind.ShouldBe(LinkKind.External);
        links[0].IsInternal.ShouldBeFalse();
    }

    [Fact]
    public void ShouldClassifyHttpExternalLinksAsExternal()
    {
        var html = "<a href=\"http://example.com/page\">HTTP</a>";

        var links = LinkExtractor.Extract(html, "/");

        links[0].Kind.ShouldBe(LinkKind.External);
    }

    [Fact]
    public void ShouldResolveSamePageFragmentLinksAgainstSourcePage()
    {
        var html = "<a href=\"#introduction\">Intro</a>";

        var links = LinkExtractor.Extract(html, "/docs/my-page/");

        links.Count.ShouldBe(1);
        links[0].Kind.ShouldBe(LinkKind.SamePageFragment);
        links[0].Path.ShouldBe("/docs/my-page/");
        links[0].Fragment.ShouldBe("introduction");
        links[0].SourcePage.ShouldBe("/docs/my-page/");
    }

    [Fact]
    public void ShouldSkipAnchorsWithoutHref()
    {
        var html = "<a name=\"anchor\">Named anchor</a><a id=\"target\">ID anchor</a>";

        var links = LinkExtractor.Extract(html, "/");

        links.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldHandleEmptyHref()
    {
        // AngleSharp filters out empty href — but let's ensure no crash
        var html = "<a href=\"\">Empty</a>";

        var links = LinkExtractor.Extract(html, "/");

        // Empty href is excluded (no href attribute in the query selector after parsing)
        links.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldHandleMultipleLinksInOnePage()
    {
        var html = """
            <a href="/docs/page-one/">Page One</a>
            <a href="/docs/page-two/">Page Two</a>
            <a href="https://external.com">External</a>
            <a href="#section">Fragment</a>
            """;

        var links = LinkExtractor.Extract(html, "/docs/current/");

        links.Count.ShouldBe(4);

        var internal1 = links.First(l => l.Href == "/docs/page-one/");
        internal1.Kind.ShouldBe(LinkKind.Internal);

        var internal2 = links.First(l => l.Href == "/docs/page-two/");
        internal2.Kind.ShouldBe(LinkKind.Internal);

        var external = links.First(l => l.Kind == LinkKind.External);
        external.Href.ShouldBe("https://external.com");

        var fragment = links.First(l => l.Kind == LinkKind.SamePageFragment);
        fragment.Fragment.ShouldBe("section");
        fragment.Path.ShouldBe("/docs/current/");
    }

    [Fact]
    public void ShouldHandleHtmlWithNoLinks()
    {
        var html = "<p>No links here.</p>";

        var links = LinkExtractor.Extract(html, "/");

        links.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldHandleEmptyHtml()
    {
        var links = LinkExtractor.Extract("", "/");

        links.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldClassifyMailtoLinkAsOther()
    {
        var html = "<a href=\"mailto:user@example.com\">Email</a>";

        var links = LinkExtractor.Extract(html, "/");

        links[0].Kind.ShouldBe(LinkKind.Other);
    }

    [Fact]
    public void ShouldNormaliseInternalLinkPathWithTrailingSlash()
    {
        var html = "<a href=\"/docs/page\">Page</a>";

        var links = LinkExtractor.Extract(html, "/");

        links[0].Path.ShouldBe("/docs/page/");
    }

    [Fact]
    public void ShouldPreserveFilePathWithExtension()
    {
        var html = "<a href=\"/downloads/file.pdf\">PDF</a>";

        var links = LinkExtractor.Extract(html, "/");

        links[0].Path.ShouldBe("/downloads/file.pdf");
    }

    [Fact]
    public void ShouldSetSourcePageOnAllLinks()
    {
        var html = """
            <a href="/other/">Other</a>
            <a href="https://example.com">External</a>
            """;

        var links = LinkExtractor.Extract(html, "/source/page/");

        links.ShouldAllBe(l => l.SourcePage == "/source/page/");
    }
}

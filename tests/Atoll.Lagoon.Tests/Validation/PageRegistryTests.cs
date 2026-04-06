using Atoll.Lagoon.Validation;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Validation;

public sealed class PageRegistryTests
{
    [Fact]
    public void ShouldFindRegisteredPage()
    {
        var registry = new PageRegistry();
        registry.Register("/docs/page/", []);

        registry.PageExists("/docs/page/").ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotFindUnregisteredPage()
    {
        var registry = new PageRegistry();

        registry.PageExists("/docs/missing/").ShouldBeFalse();
    }

    [Fact]
    public void ShouldNormalisePathWithoutTrailingSlash()
    {
        var registry = new PageRegistry();
        registry.Register("/docs/page/", []);

        // Both with and without trailing slash should match
        registry.PageExists("/docs/page").ShouldBeTrue();
        registry.PageExists("/docs/page/").ShouldBeTrue();
    }

    [Fact]
    public void ShouldNormaliseRegistrationWithoutTrailingSlash()
    {
        var registry = new PageRegistry();
        registry.Register("/docs/page", []);

        registry.PageExists("/docs/page/").ShouldBeTrue();
        registry.PageExists("/docs/page").ShouldBeTrue();
    }

    [Fact]
    public void ShouldFindAnchorOnRegisteredPage()
    {
        var registry = new PageRegistry();
        registry.Register("/docs/page/", ["introduction", "getting-started"]);

        registry.AnchorExists("/docs/page/", "introduction").ShouldBeTrue();
        registry.AnchorExists("/docs/page/", "getting-started").ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotFindMissingAnchorOnRegisteredPage()
    {
        var registry = new PageRegistry();
        registry.Register("/docs/page/", ["existing-anchor"]);

        registry.AnchorExists("/docs/page/", "nonexistent").ShouldBeFalse();
    }

    [Fact]
    public void ShouldReturnFalseForAnchorCheckOnUnregisteredPage()
    {
        var registry = new PageRegistry();

        registry.AnchorExists("/docs/missing/", "some-anchor").ShouldBeFalse();
    }

    [Fact]
    public void ShouldSupportMultiplePagesWithDifferentAnchors()
    {
        var registry = new PageRegistry();
        registry.Register("/docs/page-one/", ["intro", "summary"]);
        registry.Register("/docs/page-two/", ["overview"]);

        registry.AnchorExists("/docs/page-one/", "intro").ShouldBeTrue();
        registry.AnchorExists("/docs/page-one/", "overview").ShouldBeFalse();
        registry.AnchorExists("/docs/page-two/", "overview").ShouldBeTrue();
        registry.AnchorExists("/docs/page-two/", "intro").ShouldBeFalse();
    }

    [Fact]
    public void ShouldBeCaseInsensitiveForPagePaths()
    {
        var registry = new PageRegistry();
        registry.Register("/Docs/Page/", []);

        registry.PageExists("/docs/page/").ShouldBeTrue();
        registry.PageExists("/DOCS/PAGE/").ShouldBeTrue();
    }

    [Fact]
    public void ShouldBeCaseInsensitiveForAnchorIds()
    {
        var registry = new PageRegistry();
        registry.Register("/docs/page/", ["MyAnchor"]);

        registry.AnchorExists("/docs/page/", "myanchor").ShouldBeTrue();
        registry.AnchorExists("/docs/page/", "MYANCHOR").ShouldBeTrue();
    }

    [Fact]
    public void ShouldHandleRootPath()
    {
        var registry = new PageRegistry();
        registry.Register("/", ["hero"]);

        registry.PageExists("/").ShouldBeTrue();
        registry.AnchorExists("/", "hero").ShouldBeTrue();
    }

    [Fact]
    public void ShouldHandlePageWithNoAnchors()
    {
        var registry = new PageRegistry();
        registry.Register("/docs/page/", []);

        registry.PageExists("/docs/page/").ShouldBeTrue();
        registry.AnchorExists("/docs/page/", "any").ShouldBeFalse();
    }

    [Fact]
    public void ShouldPreserveFilePathsWithExtension()
    {
        var registry = new PageRegistry();
        registry.Register("/downloads/file.pdf", []);

        registry.PageExists("/downloads/file.pdf").ShouldBeTrue();
        // Should NOT normalise a file path by adding trailing slash
        registry.PageExists("/downloads/file.pdf/").ShouldBeFalse();
    }
}

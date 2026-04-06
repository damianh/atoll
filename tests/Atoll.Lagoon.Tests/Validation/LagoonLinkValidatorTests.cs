using Atoll.Lagoon.Validation;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Validation;

public sealed class LagoonLinkValidatorTests
{
    private static LinkValidationInput Page(string urlPath, string html, params string[] anchorIds)
        => new LinkValidationInput(urlPath, anchorIds, html);

    // ── Valid links ──

    [Fact]
    public void ShouldProduceNoErrorsForValidInternalLink()
    {
        var pages = new[]
        {
            Page("/docs/page-a/", "<a href=\"/docs/page-b/\">Go to B</a>"),
            Page("/docs/page-b/", "<p>Page B</p>"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.IsValid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.PagesScanned.ShouldBe(2);
        result.LinksChecked.ShouldBe(1);
    }

    [Fact]
    public void ShouldProduceNoErrorsForValidFragment()
    {
        var pages = new[]
        {
            Page("/docs/page-a/", "<a href=\"/docs/page-b/#intro\">Intro</a>"),
            Page("/docs/page-b/", "<p>Page B</p>", "intro"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.IsValid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldIgnoreExternalLinks()
    {
        var pages = new[]
        {
            Page("/docs/page/", "<a href=\"https://example.com\">External</a>"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.IsValid.ShouldBeTrue();
        result.LinksChecked.ShouldBe(0);
    }

    [Fact]
    public void ShouldProduceNoErrorsForEmptyPageSet()
    {
        var result = new LagoonLinkValidator().Validate([]);

        result.IsValid.ShouldBeTrue();
        result.PagesScanned.ShouldBe(0);
        result.LinksChecked.ShouldBe(0);
    }

    // ── Broken links ──

    [Fact]
    public void ShouldDetectBrokenInternalLink()
    {
        var pages = new[]
        {
            Page("/docs/page-a/", "<a href=\"/docs/nonexistent/\">Missing</a>"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ErrorKind.ShouldBe(LinkErrorKind.BrokenLink);
        result.Errors[0].SourcePage.ShouldBe("/docs/page-a/");
        result.Errors[0].TargetHref.ShouldBe("/docs/nonexistent/");
    }

    [Fact]
    public void ShouldDetectInvalidFragment()
    {
        var pages = new[]
        {
            Page("/docs/page-a/", "<a href=\"/docs/page-b/#missing-anchor\">Missing</a>"),
            Page("/docs/page-b/", "<p>Page B</p>", "existing-anchor"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ErrorKind.ShouldBe(LinkErrorKind.InvalidFragment);
        result.Errors[0].SourcePage.ShouldBe("/docs/page-a/");
        result.Errors[0].TargetHref.ShouldBe("/docs/page-b/#missing-anchor");
    }

    [Fact]
    public void ShouldNotReportFragmentErrorWhenFragmentValidationDisabled()
    {
        var pages = new[]
        {
            Page("/docs/page-a/", "<a href=\"/docs/page-b/#missing\">Link</a>"),
            Page("/docs/page-b/", "<p>Page B</p>"),
        };
        var options = new LinkValidationOptions(
            validateFragments: false,
            excludePatterns: [],
            treatAsErrors: true);

        var result = new LagoonLinkValidator().Validate(pages, options);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldSkipExcludedUrlPatterns()
    {
        var pages = new[]
        {
            Page("/docs/page/", "<a href=\"/api/v1/resource/\">API</a>"),
        };
        var options = new LinkValidationOptions(
            validateFragments: true,
            excludePatterns: ["/api/"],
            treatAsErrors: true);

        var result = new LagoonLinkValidator().Validate(pages, options);

        result.IsValid.ShouldBeTrue();
        result.LinksChecked.ShouldBe(0);
    }

    [Fact]
    public void ShouldValidateSamePageFragmentAgainstSourcePage()
    {
        var pages = new[]
        {
            Page("/docs/page/", "<a href=\"#section\">Section</a>", "section"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ShouldDetectInvalidSamePageFragment()
    {
        var pages = new[]
        {
            Page("/docs/page/", "<a href=\"#missing-section\">Missing</a>", "existing-section"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.IsValid.ShouldBeFalse();
        result.Errors[0].ErrorKind.ShouldBe(LinkErrorKind.InvalidFragment);
    }

    [Fact]
    public void ShouldReportMultipleErrorsFromOnePage()
    {
        var pages = new[]
        {
            Page("/docs/page/", """
                <a href="/docs/missing-one/">One</a>
                <a href="/docs/missing-two/">Two</a>
                """),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(2);
        result.Errors.ShouldAllBe(e => e.ErrorKind == LinkErrorKind.BrokenLink);
    }

    [Fact]
    public void ShouldValidateCrossPageLinksCorrectly()
    {
        var pages = new[]
        {
            Page("/docs/a/", "<a href=\"/docs/b/\">B</a><a href=\"/docs/c/\">C</a>"),
            Page("/docs/b/", "<a href=\"/docs/a/\">A</a>"),
            Page("/docs/c/", "<p>No links</p>"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.IsValid.ShouldBeTrue();
        result.LinksChecked.ShouldBe(3);
    }

    [Fact]
    public void ShouldReportElapsedTime()
    {
        var pages = new[]
        {
            Page("/docs/page/", "<a href=\"/docs/page/\">Self</a>"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        result.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void ShouldNotValidateExternalLinkEvenWithMatchingPattern()
    {
        var pages = new[]
        {
            Page("/docs/page/", "<a href=\"https://docs.internal.com/\">Internal-sounding external</a>"),
        };

        var result = new LagoonLinkValidator().Validate(pages);

        // External link — not checked
        result.IsValid.ShouldBeTrue();
        result.LinksChecked.ShouldBe(0);
    }
}

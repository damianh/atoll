using Atoll.Build.Content.Frontmatter;

namespace Atoll.Build.Tests.Content.Frontmatter;

public sealed class FrontmatterParserTests
{
    [Fact]
    public void ShouldExtractFrontmatterAndBody()
    {
        var content = "---\ntitle: Hello\n---\n# Hello World";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeTrue();
        result.RawFrontmatter.ShouldBe("title: Hello");
        result.Body.ShouldBe("# Hello World");
    }

    [Fact]
    public void ShouldHandleMultipleFrontmatterLines()
    {
        var content = "---\ntitle: Hello\ndate: 2026-01-01\ntags:\n  - csharp\n  - dotnet\n---\n# Content";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeTrue();
        result.RawFrontmatter.ShouldContain("title: Hello");
        result.RawFrontmatter.ShouldContain("date: 2026-01-01");
        result.RawFrontmatter.ShouldContain("- csharp");
        result.Body.ShouldBe("# Content");
    }

    [Fact]
    public void ShouldReturnEmptyFrontmatterWhenNone()
    {
        var content = "# Just Markdown\n\nNo frontmatter here.";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeFalse();
        result.RawFrontmatter.ShouldBeEmpty();
        result.Body.ShouldBe(content);
    }

    [Fact]
    public void ShouldHandleEmptyContent()
    {
        var result = FrontmatterParser.Parse("");

        result.HasFrontmatter.ShouldBeFalse();
        result.RawFrontmatter.ShouldBeEmpty();
        result.Body.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldThrowOnNullContent()
    {
        Should.Throw<ArgumentNullException>(() => FrontmatterParser.Parse(null!));
    }

    [Fact]
    public void ShouldHandleWindowsLineEndings()
    {
        var content = "---\r\ntitle: Hello\r\n---\r\n# Hello World";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeTrue();
        result.RawFrontmatter.ShouldBe("title: Hello");
        result.Body.ShouldBe("# Hello World");
    }

    [Fact]
    public void ShouldReturnBodyWhenDelimiterNotAtStart()
    {
        var content = "Some text\n---\ntitle: Hello\n---\n# Body";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeFalse();
        result.Body.ShouldBe(content);
    }

    [Fact]
    public void ShouldReturnBodyWhenNoClosingDelimiter()
    {
        var content = "---\ntitle: Hello\ndate: 2026-01-01";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeFalse();
        result.Body.ShouldBe(content);
    }

    [Fact]
    public void ShouldHandleEmptyFrontmatter()
    {
        var content = "---\n---\n# Body";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeFalse();
        result.RawFrontmatter.ShouldBeEmpty();
        result.Body.ShouldBe("# Body");
    }

    [Fact]
    public void ShouldHandleEmptyBodyAfterFrontmatter()
    {
        var content = "---\ntitle: Hello\n---\n";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeTrue();
        result.RawFrontmatter.ShouldBe("title: Hello");
        result.Body.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldHandleBodyWithMultipleDashLines()
    {
        var content = "---\ntitle: Hello\n---\n# Body\n---\nMore content after hr\n---\nEnd";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeTrue();
        result.RawFrontmatter.ShouldBe("title: Hello");
        result.Body.ShouldContain("# Body");
        result.Body.ShouldContain("---");
        result.Body.ShouldContain("More content after hr");
    }

    [Fact]
    public void ShouldHandleOnlyDelimiters()
    {
        var content = "---\n---";
        var result = FrontmatterParser.Parse(content);

        result.HasFrontmatter.ShouldBeFalse();
        result.RawFrontmatter.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldHandleContentShorterThanDelimiter()
    {
        var result = FrontmatterParser.Parse("--");

        result.HasFrontmatter.ShouldBeFalse();
        result.Body.ShouldBe("--");
    }

    [Fact]
    public void ShouldHandleDelimiterFollowedByNonNewlineCharacter()
    {
        var result = FrontmatterParser.Parse("---extra\ntitle: Hello\n---\n# Body");

        result.HasFrontmatter.ShouldBeFalse();
        result.Body.ShouldContain("---extra");
    }

    [Fact]
    public void ShouldHandleDelimiterExactlyAtEndOfContent()
    {
        // Opening delimiter is the entire content
        var result = FrontmatterParser.Parse("---");

        result.HasFrontmatter.ShouldBeFalse();
        result.Body.ShouldBe("---");
    }
}

public sealed class FrontmatterParseResultTests
{
    [Fact]
    public void ShouldThrowOnNullRawFrontmatter()
    {
        Should.Throw<ArgumentNullException>(() => new FrontmatterParseResult(null!, "body"));
    }

    [Fact]
    public void ShouldThrowOnNullBody()
    {
        Should.Throw<ArgumentNullException>(() => new FrontmatterParseResult("yaml", null!));
    }

    [Fact]
    public void HasFrontmatterShouldBeFalseForEmptyRawFrontmatter()
    {
        var result = new FrontmatterParseResult("", "body");

        result.HasFrontmatter.ShouldBeFalse();
    }

    [Fact]
    public void HasFrontmatterShouldBeTrueForNonEmptyRawFrontmatter()
    {
        var result = new FrontmatterParseResult("title: Hello", "body");

        result.HasFrontmatter.ShouldBeTrue();
    }
}

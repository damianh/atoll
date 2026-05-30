using Atoll.Lsp.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Atoll.Lsp.Tests.Parsing;

public sealed class MdaDocumentParserTests
{
    private static readonly DocumentUri TestUri = DocumentUri.File("/test/doc.mda");

    [Fact]
    public void ShouldParseEmptyDocument()
    {
        var doc = MdaDocumentParser.Parse(TestUri, string.Empty, 1);
        doc.Content.ShouldBe(string.Empty);
        doc.Frontmatter.ShouldBeNull();
        doc.Directives.ShouldBeEmpty();
        doc.Tags.ShouldBeEmpty();
        doc.Headings.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldParseFrontmatter()
    {
        var content = "---\ntitle: Hello\ndate: 2026-01-01\n---\n# Body";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        doc.Frontmatter.ShouldNotBeNull();
        doc.Frontmatter.RawYaml.ShouldContain("title: Hello");
        doc.Frontmatter.RawYaml.ShouldContain("date: 2026-01-01");
    }

    [Fact]
    public void ShouldParseBodyOnlyWithoutFrontmatter()
    {
        var content = "# My Heading\n\nSome content here.";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        doc.Frontmatter.ShouldBeNull();
        doc.Headings.Count.ShouldBe(1);
        doc.Headings[0].Text.ShouldBe("My Heading");
        doc.Headings[0].Level.ShouldBe(1);
    }

    [Fact]
    public void ShouldParseDirectiveWithProps()
    {
        var content = "# Doc\n\n:::aside{type=\"warning\" title=\"Note\"}\nContent\n:::";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        doc.Directives.Count.ShouldBe(1);
        doc.Directives[0].Name.ShouldBe("aside");
        doc.Directives[0].PropsString.ShouldContain("warning");
        doc.Directives[0].IsBlock.ShouldBeTrue();
    }

    [Fact]
    public void ShouldParseDirectiveWithoutProps()
    {
        var content = ":::card\nContent\n:::";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        doc.Directives.Count.ShouldBe(1);
        doc.Directives[0].Name.ShouldBe("card");
        doc.Directives[0].PropsString.ShouldBe(string.Empty);
    }

    [Fact]
    public void ShouldParseKebabCaseDirectiveName()
    {
        var content = ":::card-grid\nContent\n:::";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        doc.Directives.Count.ShouldBe(1);
        doc.Directives[0].Name.ShouldBe("card-grid");
    }

    [Fact]
    public void ShouldParseSelfClosingComponentTag()
    {
        var content = "# Title\n\n<Aside Type=\"warning\" />";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        doc.Tags.Count.ShouldBe(1);
        doc.Tags[0].Name.ShouldBe("Aside");
        doc.Tags[0].IsSelfClosing.ShouldBeTrue();
        doc.Tags[0].Attributes.ContainsKey("Type").ShouldBeTrue();
    }

    [Fact]
    public void ShouldParseOpeningComponentTag()
    {
        var content = "<Card Title=\"Hello\">\nContent\n</Card>";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        // At least one tag parsed (the opening <Card>)
        doc.Tags.ShouldNotBeEmpty();
        doc.Tags[0].Name.ShouldBe("Card");
        doc.Tags[0].Attributes["Title"].ShouldBe("Hello");
        doc.Tags[0].IsSelfClosing.ShouldBeFalse();
    }

    [Fact]
    public void ShouldParseHeadingsAtMultipleLevels()
    {
        var content = "# H1\n## H2\n### H3\n#### H4";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        doc.Headings.Count.ShouldBe(4);
        doc.Headings[0].Level.ShouldBe(1);
        doc.Headings[0].Text.ShouldBe("H1");
        doc.Headings[1].Level.ShouldBe(2);
        doc.Headings[2].Level.ShouldBe(3);
        doc.Headings[3].Level.ShouldBe(4);
    }

    [Fact]
    public void ShouldSkipDirectivesInsideFencedCodeBlocks()
    {
        var content = "# Doc\n\n```\n:::aside\nsome code\n:::\n```";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        doc.Directives.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldSkipTagsInsideFencedCodeBlocks()
    {
        var content = "```\n<Card />\n```";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);

        doc.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldTrackDocumentVersionAndUri()
    {
        var doc = MdaDocumentParser.Parse(TestUri, "# Test", 42);
        doc.Uri.ShouldBe(TestUri);
        doc.Version.ShouldBe(42);
    }
}

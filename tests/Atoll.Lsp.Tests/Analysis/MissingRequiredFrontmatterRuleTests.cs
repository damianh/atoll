using Atoll.Lsp.Analysis.Rules;
using Atoll.Lsp.Context;
using Atoll.Lsp.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Atoll.Lsp.Tests.Analysis;

public sealed class MissingRequiredFrontmatterRuleTests
{
    // File inside a known collection directory
    private static readonly DocumentUri InCollectionUri =
        DocumentUri.File("/workspace/content/blog/my-post.mda");

    // File outside any known collection
    private static readonly DocumentUri OutsideCollectionUri =
        DocumentUri.File("/workspace/pages/about.mda");

    private static ProjectContext CreateContextWithCollection(string collectionName,
        string directoryPath,
        params (string name, bool required)[] properties)
    {
        var schemaProperties = properties
            .Select(p => new SchemaPropertyInfo(p.name, p.name.ToLowerInvariant(), typeof(string), p.required))
            .ToList();

        var schema = new CollectionSchemaInfo(collectionName, directoryPath, schemaProperties);

        var collections = new Dictionary<string, CollectionSchemaInfo>
        {
            [collectionName] = schema,
        };

        return new ProjectContext(
            new Dictionary<string, ComponentInfo>(),
            collections,
            "content");
    }

    [Fact]
    public void ShouldReportMissingRequiredField()
    {
        var content = "---\ndate: 2026-01-01\n---\n# Post";
        var doc = MdaDocumentParser.Parse(InCollectionUri, content, 1);
        var context = CreateContextWithCollection(
            "blog",
            "content/blog",
            ("title", required: true),
            ("date", required: false));
        var rule = new MissingRequiredFrontmatterRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Code?.String.ShouldBe("atoll.missingFrontmatter");
        diagnostics[0].Message.ShouldContain("title");
    }

    [Fact]
    public void ShouldNotReportWhenRequiredFieldIsPresent()
    {
        var content = "---\ntitle: My Post\ndate: 2026-01-01\n---\n# Post";
        var doc = MdaDocumentParser.Parse(InCollectionUri, content, 1);
        var context = CreateContextWithCollection(
            "blog",
            "content/blog",
            ("title", required: true),
            ("date", required: false));
        var rule = new MissingRequiredFrontmatterRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotReportForFileOutsideKnownCollection()
    {
        var content = "---\ndate: 2026-01-01\n---\n# About";
        // File is in /workspace/pages/ but collection is in content/blog
        var doc = MdaDocumentParser.Parse(OutsideCollectionUri, content, 1);
        var context = CreateContextWithCollection(
            "blog",
            "content/blog",
            ("title", required: true));
        var rule = new MissingRequiredFrontmatterRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotReportInDegradedMode()
    {
        var content = "---\ndate: 2026-01-01\n---\n# Post";
        var doc = MdaDocumentParser.Parse(InCollectionUri, content, 1);
        var rule = new MissingRequiredFrontmatterRule();

        var diagnostics = rule.Analyze(doc, null).ToList();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotReportForDocumentWithoutFrontmatter()
    {
        var content = "# Post without frontmatter";
        var doc = MdaDocumentParser.Parse(InCollectionUri, content, 1);
        var context = CreateContextWithCollection(
            "blog",
            "content/blog",
            ("title", required: true));
        var rule = new MissingRequiredFrontmatterRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldReportMultipleMissingRequiredFields()
    {
        var content = "---\n---\n# Post";
        var doc = MdaDocumentParser.Parse(InCollectionUri, content, 1);
        var context = CreateContextWithCollection(
            "blog",
            "content/blog",
            ("title", required: true),
            ("pubDate", required: true),
            ("description", required: false));
        var rule = new MissingRequiredFrontmatterRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.Count.ShouldBe(2);
    }
}

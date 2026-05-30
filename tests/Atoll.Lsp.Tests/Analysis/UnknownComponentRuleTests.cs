using Atoll.Lsp.Analysis.Rules;
using Atoll.Lsp.Context;
using Atoll.Lsp.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Atoll.Lsp.Tests.Analysis;

public sealed class UnknownComponentRuleTests
{
    private static readonly DocumentUri TestUri = DocumentUri.File("/workspace/content/blog/post.mda");

    private static ProjectContext CreateContext(params string[] componentNames)
    {
        var components = new Dictionary<string, ComponentInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in componentNames)
        {
            var info = new ComponentInfo(name, name, null, []);
            components[name] = info;
        }

        return new ProjectContext(components,
            new Dictionary<string, CollectionSchemaInfo>(),
            "content");
    }

    [Fact]
    public void ShouldReportUnknownDirectiveComponent()
    {
        var doc = MdaDocumentParser.Parse(TestUri, ":::unknown{}\n:::", 1);
        var context = CreateContext("aside", "card");
        var rule = new UnknownComponentRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Code?.String.ShouldBe("atoll.unknownComponent");
    }

    [Fact]
    public void ShouldNotReportKnownDirectiveComponent()
    {
        var doc = MdaDocumentParser.Parse(TestUri, ":::aside{}\n:::", 1);
        var context = CreateContext("aside", "card");
        var rule = new UnknownComponentRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldReportUnknownTagComponent()
    {
        var doc = MdaDocumentParser.Parse(TestUri, "<UnknownTag />", 1);
        var context = CreateContext("aside", "card");
        var rule = new UnknownComponentRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Code?.String.ShouldBe("atoll.unknownComponent");
    }

    [Fact]
    public void ShouldNotReportKnownTagComponent()
    {
        var doc = MdaDocumentParser.Parse(TestUri, "<Card Title=\"x\" />", 1);
        var context = CreateContext("card", "Card");
        var rule = new UnknownComponentRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotReportAnythingInDegradedMode()
    {
        var doc = MdaDocumentParser.Parse(TestUri, ":::nonexistent{}\n:::", 1);
        var rule = new UnknownComponentRule();

        // context = null means degraded mode
        var diagnostics = rule.Analyze(doc, null).ToList();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldIncludeAvailableComponentsInMessage()
    {
        var doc = MdaDocumentParser.Parse(TestUri, ":::foo{}\n:::", 1);
        var context = CreateContext("aside", "card");
        var rule = new UnknownComponentRule();

        var diagnostics = rule.Analyze(doc, context).ToList();

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Message.ShouldContain("aside");
    }
}

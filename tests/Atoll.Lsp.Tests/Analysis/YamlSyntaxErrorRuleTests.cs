using Atoll.Lsp.Analysis.Rules;
using Atoll.Lsp.Context;
using Atoll.Lsp.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Atoll.Lsp.Tests.Analysis;

public sealed class YamlSyntaxErrorRuleTests
{
    private static readonly DocumentUri TestUri = DocumentUri.File("/workspace/doc.mda");

    [Fact]
    public void ShouldReportYamlSyntaxError()
    {
        // Malformed YAML: key without value followed by another mapping
        var content = "---\ntitle: Hello\nbad: :\n---\n# Body";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);
        var rule = new YamlSyntaxErrorRule();

        var diagnostics = rule.Analyze(doc, null).ToList();

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Code?.String.ShouldBe("atoll.yamlSyntaxError");
        diagnostics[0].Message.ShouldContain("YAML syntax error");
    }

    [Fact]
    public void ShouldNotReportErrorForValidYaml()
    {
        var content = "---\ntitle: Hello\ndate: 2026-01-01\ntags:\n  - foo\n  - bar\n---\n# Body";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);
        var rule = new YamlSyntaxErrorRule();

        var diagnostics = rule.Analyze(doc, null).ToList();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotReportErrorForDocumentWithoutFrontmatter()
    {
        var content = "# Just Markdown\n\nNo frontmatter here.";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);
        var rule = new YamlSyntaxErrorRule();

        var diagnostics = rule.Analyze(doc, null).ToList();

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldNotReportErrorForEmptyFrontmatter()
    {
        var content = "---\n---\n# Body";
        var doc = MdaDocumentParser.Parse(TestUri, content, 1);
        var rule = new YamlSyntaxErrorRule();

        var diagnostics = rule.Analyze(doc, null).ToList();

        diagnostics.ShouldBeEmpty();
    }
}

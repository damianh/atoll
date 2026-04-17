using Atoll.Lsp.Context;
using Atoll.Lsp.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using YamlDotNet.Core;

namespace Atoll.Lsp.Analysis.Rules;

/// <summary>
/// Reports an error for YAML syntax errors in the frontmatter section.
/// </summary>
internal sealed class YamlSyntaxErrorRule : IDiagnosticRule
{
    public IEnumerable<Diagnostic> Analyze(MdaDocument document, ProjectContext? context)
    {
        var frontmatter = document.Frontmatter;
        if (frontmatter is null || string.IsNullOrWhiteSpace(frontmatter.RawYaml))
        {
            yield break;
        }

        YamlException? yamlError = null;
        try
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            deserializer.Deserialize<Dictionary<string, object>>(frontmatter.RawYaml);
        }
        catch (YamlException ex)
        {
            yamlError = ex;
        }

        if (yamlError is not null)
        {
            // YamlDotNet Start is 1-based; LSP is 0-based. Line/Column are long in YamlDotNet v16.
            // The frontmatter YAML starts on line 1 (after the opening ---)
            var yamlLine = (int)yamlError.Start.Line - 1;
            var yamlCol = (int)yamlError.Start.Column - 1;

            // Add the frontmatter start offset (line 1, since --- is line 0)
            var diagLine = 1 + yamlLine; // +1 for the opening --- line
            var diagCol = yamlCol < 0 ? 0 : yamlCol;

            yield return new Diagnostic
            {
                Code = new DiagnosticCode("atoll.yamlSyntaxError"),
                Severity = DiagnosticSeverity.Error,
                Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                    new Position(diagLine, diagCol),
                    new Position(diagLine, diagCol + 1)),
                Message = $"YAML syntax error: {yamlError.Message}",
                Source = "atoll",
            };
        }
    }
}

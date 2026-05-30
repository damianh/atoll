using Atoll.Lsp.Analysis.Rules;
using Atoll.Lsp.Context;
using Atoll.Lsp.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Atoll.Lsp.Analysis;

/// <summary>
/// Orchestrates all <see cref="IDiagnosticRule"/> instances to produce LSP diagnostics
/// for a given <see cref="MdaDocument"/>.
/// </summary>
internal sealed class MdaDocumentAnalyzer
{
    private readonly IReadOnlyList<IDiagnosticRule> _rules;

    internal MdaDocumentAnalyzer()
    {
        _rules =
        [
            new YamlSyntaxErrorRule(),
            new MissingRequiredFrontmatterRule(),
            new UnknownComponentRule(),
        ];
    }

    /// <summary>
    /// Runs all diagnostic rules against the document and returns the combined results.
    /// </summary>
    internal IReadOnlyList<Diagnostic> Analyze(MdaDocument document, ProjectContext? context)
    {
        var diagnostics = new List<Diagnostic>();
        foreach (var rule in _rules)
        {
            diagnostics.AddRange(rule.Analyze(document, context));
        }

        return diagnostics;
    }
}

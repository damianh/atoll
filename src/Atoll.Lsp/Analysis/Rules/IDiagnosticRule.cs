using Atoll.Lsp.Context;
using Atoll.Lsp.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Atoll.Lsp.Analysis.Rules;

/// <summary>
/// Defines a diagnostic rule that can analyse an MDA document and produce LSP diagnostics.
/// </summary>
internal interface IDiagnosticRule
{
    IEnumerable<Diagnostic> Analyze(MdaDocument document, ProjectContext? context);
}

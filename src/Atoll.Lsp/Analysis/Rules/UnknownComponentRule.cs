using Atoll.Lsp.Context;
using Atoll.Lsp.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Atoll.Lsp.Analysis.Rules;

/// <summary>
/// Reports a warning for any <c>:::name</c> directive or <c>&lt;PascalCase&gt;</c> tag
/// whose name is not registered in the project's <see cref="ProjectContext.Components"/> map.
/// </summary>
internal sealed class UnknownComponentRule : IDiagnosticRule
{
    public IEnumerable<Diagnostic> Analyze(MdaDocument document, ProjectContext? context)
    {
        // In degraded mode (no context), don't report false positives
        if (context is null)
        {
            yield break;
        }

        foreach (var directive in document.Directives)
        {
            if (!context.Components.ContainsKey(directive.Name))
            {
                var available = string.Join(", ", context.Components.Keys
                    .Where(k => !char.IsUpper(k[0])) // only kebab-case names for the message
                    .Order());

                yield return new Diagnostic
                {
                    Code = new DiagnosticCode("atoll.unknownComponent"),
                    Severity = DiagnosticSeverity.Warning,
                    Range = directive.NameRange,
                    Message = string.IsNullOrEmpty(available)
                        ? $"Unknown component '{directive.Name}'. No components are registered."
                        : $"Unknown component '{directive.Name}'. Available: {available}.",
                    Source = "atoll",
                };
            }
        }

        foreach (var tag in document.Tags)
        {
            if (!context.Components.ContainsKey(tag.Name))
            {
                var available = string.Join(", ", context.Components.Keys
                    .Where(k => char.IsUpper(k[0])) // PascalCase names for tag context
                    .Order());

                yield return new Diagnostic
                {
                    Code = new DiagnosticCode("atoll.unknownComponent"),
                    Severity = DiagnosticSeverity.Warning,
                    Range = tag.NameRange,
                    Message = string.IsNullOrEmpty(available)
                        ? $"Unknown component '<{tag.Name}>'. No components are registered."
                        : $"Unknown component '<{tag.Name}>'. Available: {available}.",
                    Source = "atoll",
                };
            }
        }
    }
}

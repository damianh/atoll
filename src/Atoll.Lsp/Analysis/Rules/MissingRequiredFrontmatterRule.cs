using Atoll.Lsp.Context;
using Atoll.Lsp.Documents;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Atoll.Lsp.Analysis.Rules;

/// <summary>
/// Reports errors for required frontmatter fields that are absent in the document's YAML.
/// Only applies to files that belong to a known content collection.
/// </summary>
internal sealed class MissingRequiredFrontmatterRule : IDiagnosticRule
{
    public IEnumerable<Diagnostic> Analyze(MdaDocument document, ProjectContext? context)
    {
        if (context is null || document.Frontmatter is null)
        {
            yield break;
        }

        // Determine which collection (if any) this document belongs to
        var documentPath = document.Uri.GetFileSystemPath();
        if (documentPath is null)
        {
            yield break;
        }

        // Make path workspace-relative using forward slashes
        var workspaceRelative = GetWorkspaceRelativePath(documentPath, context);
        if (workspaceRelative is null)
        {
            yield break;
        }

        if (!context.TryGetCollectionForPath(workspaceRelative, out var schema) || schema is null)
        {
            yield break;
        }

        // Parse the YAML keys that are present
        var presentKeys = GetYamlKeys(document.Frontmatter.RawYaml);

        foreach (var prop in schema.Properties.Where(p => p.Required))
        {
            if (!presentKeys.Contains(prop.YamlKey, StringComparer.OrdinalIgnoreCase))
            {
                yield return new Diagnostic
                {
                    Code = new DiagnosticCode("atoll.missingFrontmatter"),
                    Severity = DiagnosticSeverity.Error,
                    Range = document.Frontmatter.Range,
                    Message = $"Required frontmatter field '{prop.YamlKey}' is missing.",
                    Source = "atoll",
                };
            }
        }
    }

    private static string? GetWorkspaceRelativePath(string absolutePath, ProjectContext context)
    {
        // Try to derive workspace-relative path from the base directory hint
        // We look for the baseDirectory segment in the path
        var normalized = absolutePath.Replace('\\', '/');
        var baseDir = context.BaseDirectory.Replace('\\', '/');

        // Walk up from the absolute path to find a root that contains the baseDirectory
        var idx = normalized.IndexOf('/' + baseDir + '/', StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            return normalized[(idx + 1)..];
        }

        // Fallback: use the full normalized path
        return normalized;
    }

    private static HashSet<string> GetYamlKeys(string rawYaml)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(rawYaml))
        {
            return keys;
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var dict = deserializer.Deserialize<Dictionary<string, object?>>(rawYaml);
            if (dict is not null)
            {
                foreach (var key in dict.Keys)
                {
                    keys.Add(key);
                }
            }
        }
        catch
        {
            // If YAML is malformed, the YamlSyntaxErrorRule will catch it
        }

        return keys;
    }
}

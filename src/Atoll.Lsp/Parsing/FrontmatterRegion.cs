using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Atoll.Lsp.Parsing;

/// <summary>
/// Represents the YAML frontmatter region of an MDA document.
/// </summary>
internal sealed class FrontmatterRegion
{
    internal FrontmatterRegion(
        string rawYaml,
        LspRange range,
        int bodyStartOffset)
    {
        RawYaml = rawYaml;
        Range = range;
        BodyStartOffset = bodyStartOffset;
    }

    /// <summary>Raw YAML content between the --- delimiters (without the delimiters themselves).</summary>
    internal string RawYaml { get; }

    /// <summary>LSP range covering the entire frontmatter block including --- delimiters.</summary>
    internal LspRange Range { get; }

    /// <summary>Character offset in the document where the body (after frontmatter) begins.</summary>
    internal int BodyStartOffset { get; }
}

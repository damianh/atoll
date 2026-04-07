using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Atoll.Lsp.Parsing;

/// <summary>
/// Represents a Markdown heading (<c>#</c> through <c>######</c>) found in an MDA document.
/// </summary>
internal sealed class HeadingInfo
{
    internal HeadingInfo(int level, string text, LspRange range)
    {
        Level = level;
        Text = text;
        Range = range;
    }

    /// <summary>Heading level (1–6).</summary>
    internal int Level { get; }

    /// <summary>The heading text (stripped of inline markup).</summary>
    internal string Text { get; }

    /// <summary>LSP range covering the entire heading line.</summary>
    internal LspRange Range { get; }
}

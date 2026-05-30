using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Atoll.Lsp.Parsing;

/// <summary>
/// Represents a <c>&lt;PascalCase ... &gt;</c> component tag usage found in an MDA document body.
/// </summary>
internal sealed class TagUsage
{
    internal TagUsage(
        string name,
        IReadOnlyDictionary<string, string?> attributes,
        LspRange nameRange,
        LspRange fullRange,
        bool isSelfClosing)
    {
        Name = name;
        Attributes = attributes;
        NameRange = nameRange;
        FullRange = fullRange;
        IsSelfClosing = isSelfClosing;
    }

    /// <summary>The component type name (e.g., "CardGrid", "Aside").</summary>
    internal string Name { get; }

    /// <summary>Parsed attribute key/value pairs. Value is null for boolean attributes.</summary>
    internal IReadOnlyDictionary<string, string?> Attributes { get; }

    /// <summary>LSP range covering just the tag name within the opening tag.</summary>
    internal LspRange NameRange { get; }

    /// <summary>LSP range covering the entire opening tag token.</summary>
    internal LspRange FullRange { get; }

    /// <summary><c>true</c> for <c>&lt;Tag /&gt;</c> self-closing tags.</summary>
    internal bool IsSelfClosing { get; }
}

using LspRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Atoll.Lsp.Parsing;

/// <summary>
/// Represents a <c>:::name{props}</c> directive usage found in an MDA document body.
/// </summary>
internal sealed class DirectiveUsage
{
    internal DirectiveUsage(
        string name,
        string propsString,
        LspRange nameRange,
        LspRange fullRange,
        bool isBlock)
    {
        Name = name;
        PropsString = propsString;
        NameRange = nameRange;
        FullRange = fullRange;
        IsBlock = isBlock;
    }

    /// <summary>The directive name (e.g., "aside", "card-grid").</summary>
    internal string Name { get; }

    /// <summary>The raw props string from inside the braces, e.g., <c>type="warning" title="Note"</c>.</summary>
    internal string PropsString { get; }

    /// <summary>LSP range covering just the name portion of the directive opening line.</summary>
    internal LspRange NameRange { get; }

    /// <summary>LSP range covering the entire opening line including <c>:::</c> and props.</summary>
    internal LspRange FullRange { get; }

    /// <summary>
    /// <c>true</c> if this is a block directive with a closing <c>:::</c>;
    /// <c>false</c> if it's a self-closing (empty) directive.
    /// </summary>
    internal bool IsBlock { get; }
}

using Atoll.Lsp.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Atoll.Lsp.Documents;

/// <summary>
/// An in-memory representation of an open MDA document in the editor.
/// Contains the raw content and lazily-parsed structure.
/// </summary>
internal sealed class MdaDocument
{
    internal MdaDocument(
        DocumentUri uri,
        string content,
        int version,
        LineMap lineMap,
        FrontmatterRegion? frontmatter,
        IReadOnlyList<DirectiveUsage> directives,
        IReadOnlyList<TagUsage> tags,
        IReadOnlyList<HeadingInfo> headings)
    {
        Uri = uri;
        Content = content;
        Version = version;
        LineMap = lineMap;
        Frontmatter = frontmatter;
        Directives = directives;
        Tags = tags;
        Headings = headings;
    }

    internal DocumentUri Uri { get; }
    internal string Content { get; }
    internal int Version { get; }
    internal LineMap LineMap { get; }
    internal FrontmatterRegion? Frontmatter { get; }
    internal IReadOnlyList<DirectiveUsage> Directives { get; }
    internal IReadOnlyList<TagUsage> Tags { get; }
    internal IReadOnlyList<HeadingInfo> Headings { get; }
}

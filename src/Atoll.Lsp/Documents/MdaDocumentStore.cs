using System.Collections.Concurrent;
using Atoll.Lsp.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Atoll.Lsp.Documents;

/// <summary>
/// Tracks open MDA documents in memory, keyed by URI.
/// Thread-safe for concurrent reads from the LSP dispatcher.
/// </summary>
internal sealed class MdaDocumentStore
{
    private readonly ConcurrentDictionary<DocumentUri, MdaDocument> _documents = new();

    /// <summary>
    /// Opens or updates a document, replacing any existing content.
    /// </summary>
    internal MdaDocument Update(DocumentUri uri, string content, int version)
    {
        var document = MdaDocumentParser.Parse(uri, content, version);
        _documents[uri] = document;
        return document;
    }

    /// <summary>
    /// Removes a document from the store.
    /// </summary>
    internal void Remove(DocumentUri uri) => _documents.TryRemove(uri, out _);

    /// <summary>
    /// Tries to retrieve a document by URI.
    /// </summary>
    internal bool TryGet(DocumentUri uri, out MdaDocument? document)
        => _documents.TryGetValue(uri, out document);
}

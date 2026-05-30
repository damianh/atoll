using Atoll.Lsp.Analysis;
using Atoll.Lsp.Context;
using Atoll.Lsp.Documents;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace Atoll.Lsp.Handlers;

/// <summary>
/// Handles LSP text document lifecycle events: didOpen, didChange, didClose.
/// On each open or change, re-parses the document and publishes fresh diagnostics.
/// </summary>
internal sealed class TextDocumentSyncHandler : TextDocumentSyncHandlerBase
{
    private static readonly TextDocumentSelector MdaDocumentSelector =
        new(new TextDocumentFilter { Pattern = "**/*.mda" });

    private readonly MdaDocumentStore _store;
    private readonly MdaDocumentAnalyzer _analyzer;
    private readonly ProjectContextProvider _contextProvider;
    private readonly ILanguageServerFacade _server;
    private readonly ILogger<TextDocumentSyncHandler> _logger;

    internal TextDocumentSyncHandler(
        MdaDocumentStore store,
        MdaDocumentAnalyzer analyzer,
        ProjectContextProvider contextProvider,
        ILanguageServerFacade server,
        ILogger<TextDocumentSyncHandler> logger)
    {
        _store = store;
        _analyzer = analyzer;
        _contextProvider = contextProvider;
        _server = server;
        _logger = logger;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities)
        => new()
        {
            DocumentSelector = MdaDocumentSelector,
            Change = TextDocumentSyncKind.Full,
            Save = new SaveOptions { IncludeText = false },
        };

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        => new(uri, "mda");

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        _logger.LogDebug("didOpen: {Uri}", uri);
        AnalyseAndPublish(uri, request.TextDocument.Text, request.TextDocument.Version);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        _logger.LogDebug("didChange: {Uri}", uri);

        // Full sync — only one change item with the complete text
        var text = request.ContentChanges.FirstOrDefault()?.Text ?? string.Empty;
        AnalyseAndPublish(uri, text, request.TextDocument.Version);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri;
        _logger.LogDebug("didClose: {Uri}", uri);
        _store.Remove(uri);

        // Clear diagnostics when the document is closed
        _server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
        {
            Uri = uri,
            Diagnostics = [],
        });

        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        => Unit.Task;

    private void AnalyseAndPublish(DocumentUri uri, string text, int? version)
    {
        try
        {
            var document = _store.Update(uri, text, version ?? 0);
            var context = _contextProvider.Current;
            var diagnostics = _analyzer.Analyze(document, context);

            _server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
            {
                Uri = uri,
                Version = version,
                Diagnostics = new Container<Diagnostic>(diagnostics),
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyse document {Uri}", uri);
        }
    }
}

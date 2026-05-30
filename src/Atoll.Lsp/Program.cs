using Atoll.Lsp.Analysis;
using Atoll.Lsp.Context;
using Atoll.Lsp.Documents;
using Atoll.Lsp.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;

var server = await LanguageServer.From(options =>
{
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardOutput())
        .ConfigureLogging(x => x
            .AddLanguageProtocolLogging()
            .SetMinimumLevel(LogLevel.Warning))
        .WithHandler<TextDocumentSyncHandler>()
        .WithServices(services =>
        {
            services.AddSingleton<MdaDocumentStore>();
            services.AddSingleton<MdaDocumentAnalyzer>();
            services.AddSingleton<ProjectContextProvider>();
        });
}).ConfigureAwait(false);

await server.WaitForExit.ConfigureAwait(false);

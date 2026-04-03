using Atoll.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Atoll.Cli.Commands;

/// <summary>
/// Handles the <c>atoll preview</c> command. Serves the built <c>dist/</c>
/// directory as a static file server for local testing before deployment.
/// </summary>
public sealed class PreviewCommandHandler
{
    /// <summary>
    /// Executes the preview command.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    /// <param name="port">The port override (0 = use config default).</param>
    public async Task ExecuteAsync(string projectRoot, int port)
    {
        var config = await AtollConfigLoader.LoadAsync(projectRoot);
        var outputDir = AtollConfigLoader.ResolveOutputDirectory(config, projectRoot);
        var effectivePort = port > 0 ? port : config.Server.Port;

        if (!Directory.Exists(outputDir))
        {
            Console.WriteLine($"Error: Output directory '{outputDir}' does not exist.");
            Console.WriteLine("Run 'atoll build' first to generate the site.");
            return;
        }

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://{config.Server.Host}:{effectivePort}");

        var app = builder.Build();

        // Serve static files from the output directory
        var fileProvider = new PhysicalFileProvider(Path.GetFullPath(outputDir));
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });

        // Fallback: serve index.html for SPA-style routing
        app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = fileProvider });

        Console.WriteLine($"Atoll — preview server starting on http://{config.Server.Host}:{effectivePort}");
        Console.WriteLine($"  Serving: {outputDir}");
        Console.WriteLine("  Press Ctrl+C to stop.");

        await app.RunAsync();
    }
}

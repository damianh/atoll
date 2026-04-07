using Atoll.Build.Pipeline;
using Atoll.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

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
    /// <param name="cancellationToken">A token to cancel the preview operation.</param>
    public async Task ExecuteAsync(string projectRoot, int port, CancellationToken cancellationToken)
    {
        var config = await AtollConfigLoader.LoadAsync(projectRoot, cancellationToken);
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

        // Serve static files from the output directory with appropriate Cache-Control headers
        var fileProvider = new PhysicalFileProvider(Path.GetFullPath(outputDir));

        var staticFileOptions = new StaticFileOptions
        {
            FileProvider = fileProvider,
            OnPrepareResponse = ctx =>
            {
                var path = ctx.Context.Request.Path.Value ?? "";
                var cacheControl = ResolveCacheControl(path);
                ctx.Context.Response.Headers["Cache-Control"] = cacheControl;

                // Vary: Accept-Encoding for compressible content types
                var contentType = ctx.Context.Response.ContentType ?? "";
                if (IsCompressibleContentType(contentType))
                {
                    ctx.Context.Response.Headers["Vary"] = "Accept-Encoding";
                }
            },
        };

        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
        app.UseStaticFiles(staticFileOptions);

        // Fallback: serve index.html for SPA-style routing
        app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = fileProvider });

        Console.WriteLine($"Atoll — preview server starting on http://{config.Server.Host}:{effectivePort}");
        Console.WriteLine($"  Serving: {outputDir}");
        Console.WriteLine("  Press Ctrl+C to stop.");

        await ((IHost)app).RunAsync(cancellationToken);
    }

    /// <summary>
    /// Resolves the <c>Cache-Control</c> header value for a given request path.
    /// </summary>
    private static string ResolveCacheControl(string path)
    {
        // Fingerprinted assets under /_atoll/ are immutable — safe to cache for 1 year
        if (FingerprintDetector.IsFingerprintedAsset(path))
        {
            return "public, max-age=31536000, immutable";
        }

        // HTML pages and the root should always revalidate
        if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("/", StringComparison.OrdinalIgnoreCase)
            || path == "")
        {
            return "public, max-age=0, must-revalidate";
        }

        // Default: cache for 1 hour
        return "public, max-age=3600";
    }

    /// <summary>
    /// Returns <c>true</c> for content types that benefit from compression and
    /// therefore need a <c>Vary: Accept-Encoding</c> response header.
    /// </summary>
    private static bool IsCompressibleContentType(string contentType)
    {
        return contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/javascript", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/xml", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("image/svg+xml", StringComparison.OrdinalIgnoreCase);
    }
}


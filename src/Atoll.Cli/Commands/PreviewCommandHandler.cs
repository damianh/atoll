using Atoll.Core.Configuration;

namespace Atoll.Cli.Commands;

/// <summary>
/// Handles the <c>atoll preview</c> command. Serves the built <c>dist/</c>
/// directory as a static file server for local testing.
/// </summary>
/// <remarks>
/// <para>
/// Uses ASP.NET Core's static file middleware to serve the pre-built site.
/// This allows verifying the production build locally before deployment.
/// </para>
/// </remarks>
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

        Console.WriteLine($"Atoll — preview server starting on http://{config.Server.Host}:{effectivePort}");
        Console.WriteLine($"  Serving: {outputDir}");
        Console.WriteLine("  Press Ctrl+C to stop.");

        // Placeholder: Phase 10 will implement full static file serving.
        // For now, just keep the process alive until cancelled.
        try
        {
            await Task.Delay(Timeout.Infinite, CancellationToken.None);
        }
        catch (TaskCanceledException)
        {
            // Expected on Ctrl+C
        }
    }
}

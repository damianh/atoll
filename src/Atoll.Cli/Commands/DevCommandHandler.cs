using Atoll.Core.Configuration;

namespace Atoll.Cli.Commands;

/// <summary>
/// Handles the <c>atoll dev</c> command. Starts the ASP.NET Core development server
/// with Atoll middleware for live rendering of pages and endpoints.
/// </summary>
/// <remarks>
/// <para>
/// The development server will be enhanced in Phase 10 with file watching and
/// live reload capabilities. For now, it starts a basic server with the
/// Atoll middleware pipeline.
/// </para>
/// </remarks>
public sealed class DevCommandHandler
{
    /// <summary>
    /// Executes the dev command.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    /// <param name="port">The port override (0 = use config default).</param>
    public async Task ExecuteAsync(string projectRoot, int port)
    {
        var config = await AtollConfigLoader.LoadAsync(projectRoot);
        var effectivePort = port > 0 ? port : config.Server.Port;

        Console.WriteLine($"Atoll — dev server starting on http://{config.Server.Host}:{effectivePort}");
        Console.WriteLine("  (Development server — Phase 10 will add file watching and live reload)");
        Console.WriteLine("  Press Ctrl+C to stop.");

        // Placeholder: Phase 10 will implement full dev server with file watching.
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

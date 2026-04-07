using Atoll.Configuration;
using Atoll.Middleware.Server.DevServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Atoll.Cli.Commands;

/// <summary>
/// Handles the <c>atoll dev</c> command. Starts the ASP.NET Core development server
/// with Atoll middleware for live rendering of pages and endpoints, and watches for
/// file changes to hot-reload routes and content without restarting the listener.
/// Browser clients are notified via WebSocket after each successful reload.
/// </summary>
public sealed class DevCommandHandler
{
    /// <summary>
    /// Executes the dev command.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    /// <param name="port">The port override (0 = use config default).</param>
    /// <param name="cancellationToken">A token to cancel the dev server operation.</param>
    public async Task ExecuteAsync(string projectRoot, int port, CancellationToken cancellationToken)
    {
        var config = await AtollConfigLoader.LoadAsync(projectRoot, cancellationToken);
        var effectivePort = port > 0 ? port : config.Server.Port;

        var csprojPath = FindProjectFile(projectRoot);

        // Create the live-reload handler unconditionally — the middleware injects
        // the reconnecting WebSocket script into every HTML response so the browser
        // is ready to receive notifications as soon as the server starts.
        using var liveReloadHandler = new LiveReloadWebSocketHandler();

        // Build the web application host so we can obtain a logger factory.
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://{config.Server.Host}:{effectivePort}");

        // Suppress Kestrel and ASP.NET Core infrastructure noise while allowing
        // our own request-logging middleware to emit at Information level.
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Logging.AddFilter<Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider>(
            "Atoll.Middleware.Server.DevServer.DevRequestLoggingMiddleware",
            LogLevel.Information);

        // Register the live-reload handler so LiveReloadMiddleware can resolve it.
        builder.Services.AddSingleton(liveReloadHandler);

        var app = builder.Build();
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

        // ── Request logging ──────────────────────────────────────────────────
        app.UseMiddleware<DevRequestLoggingMiddleware>();

        // ── Live-reload WebSocket endpoint + HTML script injection ────────────
        // Must be registered before the DevAtollRequestHandler lambda so that:
        //   (a) WebSocket upgrades at /__atoll-live-reload are intercepted first
        //   (b) HTML responses produced by the handler are wrapped and injected
        app.UseWebSockets();
        app.UseMiddleware<LiveReloadMiddleware>();

        // ── Build initial state ───────────────────────────────────────────────

        if (csprojPath is not null)
        {
            var reloaderLogger = loggerFactory.CreateLogger<Dev.DevServerReloader>();
            var reloader = new Dev.DevServerReloader(projectRoot, csprojPath, reloaderLogger);
            var (initialState, initialBuildError) = await reloader.BuildInitialStateAsync();
            if (initialBuildError is not null)
            {
                Console.WriteLine($"  Initial build had errors:\n{initialBuildError}");
            }

            // ── Wire file-watching + hot-reload ───────────────────────────────

            var handlerLogger = loggerFactory.CreateLogger<Dev.DevAtollRequestHandler>();
            var handler = new Dev.DevAtollRequestHandler(initialState, handlerLogger);

            // Register handler so the middleware lambda can capture it.
            app.Use(async (context, next) =>
            {
                if (!await handler.TryHandleAsync(context))
                {
                    await next(context);
                }
            });

            Console.WriteLine($"Atoll — dev server starting on http://{config.Server.Host}:{effectivePort}");
            Console.WriteLine("  Press Ctrl+C to stop.");
            Console.WriteLine("  Watching for file changes (.cs, .md, atoll.json)");

            var semaphore = new SemaphoreSlim(1, 1);

            using var watcher = new Dev.DevFileWatcher(projectRoot);
            watcher.OnChange += async kind =>
            {
                // Serialize reload operations — prevent concurrent rebuilds from racing.
                await semaphore.WaitAsync();
                try
                {
                    var (newState, buildError) = await reloader.ReloadAsync(handler.CurrentState, kind);
                    handler.UpdateState(newState);

                    if (buildError is not null)
                    {
                        // Surface build errors in the browser via an error overlay.
                        Console.WriteLine($"  Build errors:\n{buildError}");
                        await liveReloadHandler.NotifyBuildErrorAsync(buildError);
                        Console.WriteLine("  Build error notification sent to browser.");
                    }
                    else
                    {
                        // Notify all connected browser clients to refresh. Both code
                        // changes and content-only changes require a full page reload
                        // because the rendered HTML has changed.
                        await liveReloadHandler.NotifyReloadAsync();
                        Console.WriteLine("  Browser reload notification sent.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Warning: Reload failed — {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            };

            watcher.Start();

            await ((IHost)app).RunAsync(cancellationToken);
        }
        else
        {
            // No project file — serve an empty response (still starts the listener).
            // Live-reload middleware is still active so the browser script connects
            // and will be notified if the server is restarted with a project present.
            var emptyState = Dev.DevServerState.Empty;
            var handlerLogger = loggerFactory.CreateLogger<Dev.DevAtollRequestHandler>();
            var handler = new Dev.DevAtollRequestHandler(emptyState, handlerLogger);

            app.Use(async (context, next) =>
            {
                if (!await handler.TryHandleAsync(context))
                {
                    await next(context);
                }
            });

            Console.WriteLine($"Atoll — dev server starting on http://{config.Server.Host}:{effectivePort}");
            Console.WriteLine("  Warning: No .csproj found — starting with no routes.");
            Console.WriteLine("  Press Ctrl+C to stop.");

            await ((IHost)app).RunAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Finds the <c>.csproj</c> file in the project root directory.
    /// </summary>
    private static string? FindProjectFile(string projectRoot)
    {
        var csprojFiles = Directory.GetFiles(projectRoot, "*.csproj", SearchOption.TopDirectoryOnly);
        return csprojFiles.Length > 0 ? csprojFiles[0] : null;
    }
}

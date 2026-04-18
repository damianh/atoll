using Atoll.Configuration;
using Atoll.Middleware.Server.DevServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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
    /// Executes the dev command without writing to the output directory.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    /// <param name="port">The port override (0 = use config default).</param>
    /// <param name="cancellationToken">A token to cancel the dev server operation.</param>
    public Task ExecuteAsync(string projectRoot, int port, CancellationToken cancellationToken)
        => ExecuteAsync(projectRoot, port, writeDist: false, cancellationToken);

    /// <summary>
    /// Executes the dev command.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    /// <param name="port">The port override (0 = use config default).</param>
    /// <param name="writeDist">
    /// When <see langword="true"/>, all rendered pages and assets are written to the output
    /// directory after each rebuild cycle. Useful when an external process (e.g. an AppHost)
    /// needs to serve the site from disk during development.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the dev server operation.</param>
    public async Task ExecuteAsync(string projectRoot, int port, bool writeDist, CancellationToken cancellationToken)
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
            DevRequestLoggingMiddleware.LogCategory,
            LogLevel.Information);

        // Register the live-reload handler so LiveReloadMiddleware can resolve it.
        builder.Services.AddSingleton(liveReloadHandler);

        var app = builder.Build();
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

        // Resolve the public/ directory early — needed for both the static files middleware
        // and (when --write-dist is active) for copying assets to the output directory.
        var publicDir = AtollConfigLoader.ResolvePublicDirectory(config, projectRoot);

        // When --write-dist is enabled, create the writer and clean the output directory
        // once before the initial build so the consumer starts with a clean slate.
        Dev.DevDistWriter? distWriter = null;
        if (writeDist)
        {
            var outputDir = AtollConfigLoader.ResolveOutputDirectory(config, projectRoot);
            var distWriterLogger = loggerFactory.CreateLogger<Dev.DevDistWriter>();
            distWriter = new Dev.DevDistWriter(outputDir, publicDir, distWriterLogger);
            distWriter.Clean();
            Console.WriteLine($"  --write-dist: output directory {outputDir}");
        }

        // ── Request logging ──────────────────────────────────────────────────
        app.UseMiddleware<DevRequestLoggingMiddleware>();

        // ── Live-reload WebSocket endpoint + HTML script injection ────────────
        // Must be registered before the DevAtollRequestHandler lambda so that:
        //   (a) WebSocket upgrades at /__atoll-live-reload are intercepted first
        //   (b) HTML responses produced by the handler are wrapped and injected
        app.UseWebSockets();
        app.UseMiddleware<LiveReloadMiddleware>();

        // ── "Building…" gate ─────────────────────────────────────────────────
        // The server starts listening immediately so orchestrators can connect,
        // but all requests (including the health endpoint) receive a 503 with a
        // friendly page until the initial build completes. Aspire sees the
        // resource as "Starting" until the health check returns 200.
        var buildingGate = new BuildingGate();
        app.Use(async (context, next) =>
        {
            if (!buildingGate.IsReady)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.Headers["Retry-After"] = "2";
                await context.Response.WriteAsync(BuildingPage.Html, context.RequestAborted);
                return;
            }

            await next(context);
        });

        // ── Health endpoint for orchestrators (e.g. Aspire) ─────────────────
        // Placed after the building gate so it only returns 200 once the site
        // is ready to serve requests.
        app.MapGet("/__health", () => Results.Ok("healthy"));

        // ── Static files from public/ directory ──────────────────────────────
        // Resolved from atoll.json (same as build mode) — registered only when
        // the directory exists so we don't add middleware for missing dirs.
        if (Directory.Exists(publicDir))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(publicDir),
                RequestPath = "",
                OnPrepareResponse = ctx =>
                {
                    // Disable browser caching in dev so changes are always picked up.
                    ctx.Context.Response.Headers["Cache-Control"] = "no-cache";
                },
            });
        }

        // ── Atoll request handler ────────────────────────────────────────────
        // Created with an empty state initially. Once the build completes,
        // the state is swapped in atomically.
        var handlerLogger = loggerFactory.CreateLogger<Dev.DevAtollRequestHandler>();
        var handler = new Dev.DevAtollRequestHandler(Dev.DevServerState.Empty, handlerLogger);

        app.Use(async (context, next) =>
        {
            if (!await handler.TryHandleAsync(context))
            {
                await next(context);
            }
        });

        // ── Start the server immediately ─────────────────────────────────────
        // The listener is available for health checks right away. Content
        // requests get a 503 "Building…" page until the initial build completes.
        Console.WriteLine($"Atoll ({CliInfo.Version}) — dev server starting on http://{config.Server.Host}:{effectivePort}");
        Console.WriteLine("  Press Ctrl+C to stop.");

        await ((IHost)app).StartAsync(cancellationToken);

        // ── Build initial state ───────────────────────────────────────────────

        if (csprojPath is not null)
        {
            Console.WriteLine("  Building site...");

            var reloaderLogger = loggerFactory.CreateLogger<Dev.DevServerReloader>();
            var reloader = new Dev.DevServerReloader(projectRoot, csprojPath, reloaderLogger);
            var (initialState, initialBuildError) = await reloader.BuildInitialStateAsync();
            if (initialBuildError is not null)
            {
                Console.WriteLine($"  Initial build had errors:\n{initialBuildError}");
            }

            handler.UpdateState(initialState);

            // Write initial state to dist/ if --write-dist is enabled and build succeeded.
            // This must complete BEFORE we open the building gate so that orchestrators
            // (e.g. Aspire WaitFor) don't start consumers until dist/ is fully populated.
            if (distWriter is not null && initialBuildError is null)
            {
                try { await distWriter.WriteAsync(initialState, CancellationToken.None); }
                catch (Exception ex) { Console.WriteLine($"  --write-dist: error — {ex.Message}"); }
            }

            buildingGate.MarkReady();

            // ── Wire file-watching + hot-reload ───────────────────────────────

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

                        // Write updated dist/ in the background — does not block live-reload.
                        if (distWriter is not null)
                        {
                            _ = Task.Run(async () =>
                            {
                                try { await distWriter.WriteAsync(newState, CancellationToken.None); }
                                catch (Exception ex) { Console.WriteLine($"  --write-dist: error — {ex.Message}"); }
                            });
                        }
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
        }
        else
        {
            // No project file — open the gate immediately with empty state.
            buildingGate.MarkReady();

            Console.WriteLine("  Warning: No .csproj found — starting with no routes.");

            if (distWriter is not null)
            {
                Console.WriteLine("  --write-dist: no project found — dist/ will be empty.");
            }
        }

        // ── Block until shutdown ─────────────────────────────────────────────
        await ((IHost)app).WaitForShutdownAsync(cancellationToken);
        Console.WriteLine("Exiting...");
    }

    /// <summary>
    /// Thread-safe gate that tracks whether the initial site build has completed.
    /// While the gate is closed, the middleware returns a 503 "Building…" page
    /// for all non-health-check requests.
    /// </summary>
    private sealed class BuildingGate
    {
        private volatile bool _isReady;

        /// <summary>
        /// Gets a value indicating whether the initial build has completed.
        /// </summary>
        public bool IsReady => _isReady;

        /// <summary>
        /// Marks the gate as ready, allowing requests to pass through.
        /// </summary>
        public void MarkReady() => _isReady = true;
    }

    /// <summary>
    /// Provides the static HTML page shown while the site is building.
    /// </summary>
    private static class BuildingPage
    {
        /// <summary>
        /// A self-contained HTML page with a "Building site…" message and a
        /// spinner animation. The page auto-refreshes every 2 seconds so the
        /// browser transitions to the real site once the build completes.
        /// </summary>
        public const string Html =
            """
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <meta http-equiv="refresh" content="2">
              <title>Atoll — Building…</title>
              <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body {
                  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
                  background: #1a1a2e;
                  color: #eee;
                  min-height: 100vh;
                  display: flex;
                  align-items: center;
                  justify-content: center;
                }
                .container {
                  text-align: center;
                  padding: 2rem;
                }
                .spinner {
                  width: 48px;
                  height: 48px;
                  border: 4px solid #233554;
                  border-top-color: #53c0f5;
                  border-radius: 50%;
                  animation: spin 0.8s linear infinite;
                  margin: 0 auto 1.5rem;
                }
                @keyframes spin {
                  to { transform: rotate(360deg); }
                }
                h1 {
                  font-size: 1.5rem;
                  font-weight: 600;
                  margin-bottom: 0.5rem;
                  color: #53c0f5;
                }
                p {
                  color: #a3b8d4;
                  font-size: 0.95rem;
                }
              </style>
            </head>
            <body>
              <div class="container">
                <div class="spinner"></div>
                <h1>Building site…</h1>
                <p>The Atoll dev server is compiling your site. This page will refresh automatically.</p>
              </div>
            </body>
            </html>
            """;
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

using System.Collections.ObjectModel;
using System.Diagnostics;
using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;
using Atoll.Components;
using Atoll.Css;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Routing;
using Atoll.Routing.Matching;
using Microsoft.Extensions.Logging;

namespace Atoll.Cli.Commands.Dev;

/// <summary>
/// Writes all rendered pages and static assets from a <see cref="DevServerState"/> snapshot
/// to the output directory on disk. Used by <c>atoll dev --write-dist</c> to keep the
/// output directory synchronized with the dev server state after each rebuild cycle.
/// </summary>
/// <remarks>
/// <para>
/// On each call to <see cref="WriteAsync"/>, <see cref="DevDistWriter"/>:
/// </para>
/// <list type="number">
/// <item>Expands all routes (static and dynamic) to concrete URL paths via <see cref="RouteEnumerator"/>.</item>
/// <item>Renders each page component to HTML (same inline-CSS strategy as the dev server).</item>
/// <item>Writes HTML pages to disk via <see cref="OutputWriter"/>.</item>
/// <item>Writes island JavaScript assets to disk.</item>
/// <item>Writes the search index JSON file (if present).</item>
/// <item>Writes the core Atoll framework scripts (<c>_atoll/island.js</c>, <c>_atoll/directives.js</c>).</item>
/// <item>Deletes any files written in the previous cycle that are no longer present (stale file cleanup).</item>
/// </list>
/// <para>
/// Concurrent calls are serialized internally — if a new write is requested while one is already
/// in progress, the new call waits for the previous one to finish before proceeding.
/// </para>
/// </remarks>
internal sealed class DevDistWriter
{
    private readonly string _outputDirectory;
    private readonly string? _publicDirectory;
    private readonly ILogger<DevDistWriter> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private HashSet<string> _previousWrittenFiles = new(StringComparer.OrdinalIgnoreCase);

    // Incremental cache state — updated atomically within the semaphore lock.
    private string _previousAssemblyHash = "";
    private string _previousContentHash = "";
    private Dictionary<string, string> _previousRouteOutputPaths = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new <see cref="DevDistWriter"/>.
    /// </summary>
    /// <param name="outputDirectory">The absolute path to the output directory (e.g., <c>dist/</c>).</param>
    /// <param name="logger">The logger instance.</param>
    public DevDistWriter(string outputDirectory, ILogger<DevDistWriter> logger)
        : this(outputDirectory, publicDirectory: null, logger)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="DevDistWriter"/>.
    /// </summary>
    /// <param name="outputDirectory">The absolute path to the output directory (e.g., <c>dist/</c>).</param>
    /// <param name="publicDirectory">
    /// The absolute path to the <c>public/</c> directory whose files are copied to the output
    /// directory, or <c>null</c> if no public directory is configured.
    /// </param>
    /// <param name="logger">The logger instance.</param>
    public DevDistWriter(string outputDirectory, string? publicDirectory, ILogger<DevDistWriter> logger)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        ArgumentNullException.ThrowIfNull(logger);
        _outputDirectory = outputDirectory;
        _publicDirectory = publicDirectory;
        _logger = logger;
    }

    /// <summary>
    /// Performs an initial clean of the output directory, removing all existing content
    /// and recreating the directory. Called once on startup before the first write.
    /// </summary>
    public void Clean()
    {
        var writer = new OutputWriter(_outputDirectory);
        writer.Clean();
        _logger.LogDebug("Output directory cleaned: {OutputDirectory}", _outputDirectory);
    }

    /// <summary>
    /// Writes all pages and assets from the given <see cref="DevServerState"/> to disk.
    /// Serializes concurrent calls — at most one write is active at any time.
    /// </summary>
    /// <param name="state">The current dev server state snapshot.</param>
    /// <param name="cancellationToken">A token to cancel the write operation.</param>
    public async Task WriteAsync(DevServerState state, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await WriteInternalAsync(state, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // ── Private implementation ──────────────────────────────────────────────────

    private async Task WriteInternalAsync(DevServerState state, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var writer = new OutputWriter(_outputDirectory);
        var currentWrittenFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentRouteOutputPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var pageCount = 0;
        var skippedCount = 0;
        var assetCount = 0;

        // Compute incremental cache hashes for this cycle.
        var assemblyHash = state.UserAssembly?.Location is { Length: > 0 } loc
            ? InputHasher.HashAssembly(loc)
            : "";
        var contentHash = state.ContentBaseDirectory is { Length: > 0 } dir
            ? InputHasher.HashDirectory(dir)
            : "";

        var assemblyUnchanged = assemblyHash.Length > 0 && assemblyHash == _previousAssemblyHash;
        var contentUnchanged = contentHash == _previousContentHash;

        // ── 1. Expand routes and render pages ─────────────────────────────────

        IReadOnlyList<SsgRoute> ssgRoutes;
        try
        {
            var serviceProps = BuildServiceProps(state.Options);
            var enumerator = new RouteEnumerator(serviceProps);
            ssgRoutes = await enumerator.EnumerateAsync(state.RouteMatcher.SortedRoutes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "--write-dist: Failed to enumerate routes");
            return;
        }

        foreach (var route in ssgRoutes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Check whether this page can be skipped.
            if (assemblyUnchanged && _previousRouteOutputPaths.TryGetValue(route.UrlPath, out var prevOutputPath))
            {
                var isDynamic = InputHasher.IsDynamicRoute(route.ComponentType);
                var pageContentUnchanged = !isDynamic || contentUnchanged;

                if (pageContentUnchanged && File.Exists(prevOutputPath))
                {
                    currentWrittenFiles.Add(prevOutputPath);
                    currentRouteOutputPaths[route.UrlPath] = prevOutputPath;
                    skippedCount++;
                    continue;
                }
            }

            var html = await RenderPageToHtmlAsync(route, state);
            if (html is null)
            {
                continue;
            }

            try
            {
                var outputPath = await writer.WritePageAsync(route.UrlPath, html, cancellationToken);
                currentWrittenFiles.Add(outputPath);
                currentRouteOutputPaths[route.UrlPath] = outputPath;
                pageCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "--write-dist: Failed to write page {UrlPath}", route.UrlPath);
            }
        }

        // ── 2. Write island JavaScript assets ─────────────────────────────────

        foreach (var (urlKey, bytes) in state.IslandAssets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var outputPath = await writer.WriteBinaryFileAsync(urlKey, bytes, cancellationToken);
                currentWrittenFiles.Add(outputPath);
                assetCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "--write-dist: Failed to write island asset {Key}", urlKey);
            }
        }

        // ── 3. Write search index JSON ─────────────────────────────────────────

        if (state.SearchIndexJson is not null)
        {
            try
            {
                var outputPath = await writer.WriteBinaryFileAsync("search-index.json", state.SearchIndexJson, cancellationToken);
                currentWrittenFiles.Add(outputPath);
                assetCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "--write-dist: Failed to write search-index.json");
            }
        }

        // ── 4. Write core Atoll framework scripts ──────────────────────────────

        var islandScript = IslandScriptProvider.GetIslandScript();
        try
        {
            var outputPath = await writer.WriteFileAsync("_atoll/island.js", islandScript, cancellationToken);
            currentWrittenFiles.Add(outputPath);
            assetCount++;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "--write-dist: Failed to write _atoll/island.js");
        }

        var directivesScript = IslandScriptProvider.GetDirectivesScript();
        try
        {
            var outputPath = await writer.WriteFileAsync("_atoll/directives.js", directivesScript, cancellationToken);
            currentWrittenFiles.Add(outputPath);
            assetCount++;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "--write-dist: Failed to write _atoll/directives.js");
        }

        // ── 5. Write Lagoon logo (if available) ────────────────────────────────

        var logoPng = GetLagoonLogoPng();
        if (logoPng is not null)
        {
            try
            {
                var outputPath = await writer.WriteBinaryFileAsync("_atoll/logo.png", logoPng, cancellationToken);
                currentWrittenFiles.Add(outputPath);
                assetCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "--write-dist: Failed to write _atoll/logo.png");
            }
        }

        // ── 6. Copy public/ directory assets ──────────────────────────────────

        if (_publicDirectory is not null && Directory.Exists(_publicDirectory))
        {
            try
            {
                var copier = new StaticAssetCopier(_outputDirectory);
                var copyResult = await copier.CopyAsync(_publicDirectory, cancellationToken);
                foreach (var file in copyResult.Files)
                {
                    currentWrittenFiles.Add(file.DestinationPath);
                    assetCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "--write-dist: Failed to copy public/ directory assets");
            }
        }

        // ── 7. Clean stale files ───────────────────────────────────────────────

        var staleCount = 0;
        foreach (var stalePath in _previousWrittenFiles)
        {
            if (!currentWrittenFiles.Contains(stalePath))
            {
                try
                {
                    File.Delete(stalePath);
                    staleCount++;
                    _logger.LogDebug("--write-dist: Deleted stale file {Path}", stalePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "--write-dist: Failed to delete stale file {Path}", stalePath);
                }
            }
        }

        _previousWrittenFiles = currentWrittenFiles;
        _previousAssemblyHash = assemblyHash;
        _previousContentHash = contentHash;
        _previousRouteOutputPaths = currentRouteOutputPaths;

        sw.Stop();
        var skipSuffix = skippedCount > 0 ? $", {skippedCount} skipped" : "";
        Console.WriteLine($"  --write-dist: {pageCount} pages{skipSuffix}, {assetCount} assets written to {_outputDirectory} ({sw.ElapsedMilliseconds}ms){(staleCount > 0 ? $", {staleCount} stale file(s) removed" : "")}");
    }

    /// <summary>
    /// Renders a single page component to HTML using inline CSS injection,
    /// matching the dev server's rendering strategy.
    /// Returns <see langword="null"/> if rendering fails (error is logged).
    /// </summary>
    private async Task<string?> RenderPageToHtmlAsync(SsgRoute route, DevServerState state)
    {
        try
        {
            var componentType = route.ComponentType;
            var props = BuildPageProps(route, state.Options);

            var renderer = new PageRenderer();

            ComponentDelegate renderDelegate = async context =>
            {
                var component = (IAtollComponent)Activator.CreateInstance(componentType)!;

                var pageFragment = RenderFragment.FromAsync(async destination =>
                {
                    await ComponentRenderer.RenderComponentAsync(component, destination, props);
                });

                var wrappedFragment = LayoutResolver.WrapWithLayouts(componentType, pageFragment, props);
                await context.RenderAsync(wrappedFragment);

                if (component is IPageStatusCodeProvider statusProvider)
                {
                    renderer.StatusCode = statusProvider.ResponseStatusCode;
                }
            };

            // Inject global CSS inline — same strategy as the dev server.
            if (state.GlobalCss.Length > 0)
            {
                renderer.HeadManager.Add(CssInjector.CreateInlineStyle(state.GlobalCss));
            }

            var result = await renderer.RenderPageAsync(renderDelegate);
            return result.Html;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "--write-dist: Failed to render page {UrlPath} — skipping", route.UrlPath);
            return null;
        }
    }

    private static IReadOnlyDictionary<string, object?> BuildPageProps(
        SsgRoute route,
        Atoll.Middleware.Server.Hosting.AtollOptions options)
    {
        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in options.ServiceProps)
        {
            props[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in route.Parameters)
        {
            props[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in route.Props)
        {
            props[kvp.Key] = kvp.Value;
        }

        return new ReadOnlyDictionary<string, object?>(props);
    }

    private static IReadOnlyDictionary<string, object?> BuildServiceProps(
        Atoll.Middleware.Server.Hosting.AtollOptions options)
    {
        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in options.ServiceProps)
        {
            props[kvp.Key] = kvp.Value;
        }
        return new ReadOnlyDictionary<string, object?>(props);
    }

    private static byte[]? _cachedLogoPng;

    /// <summary>
    /// Loads the Atoll logo PNG from the <c>Atoll.Lagoon</c> assembly via reflection.
    /// Returns <see langword="null"/> if the assembly or resource is not available.
    /// </summary>
    private static byte[]? GetLagoonLogoPng()
    {
        if (_cachedLogoPng is not null)
        {
            return _cachedLogoPng;
        }

        try
        {
            var lagoonAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Atoll.Lagoon");

            if (lagoonAssembly is null)
            {
                return null;
            }

            const string resourceName = "Atoll.Lagoon.Assets.logo.png";
            using var stream = lagoonAssembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                return null;
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            _cachedLogoPng = ms.ToArray();
            return _cachedLogoPng;
        }
        catch
        {
            return null;
        }
    }
}

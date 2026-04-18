using System.Collections.ObjectModel;
using System.Diagnostics;
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using RazorSlices;

namespace Atoll.Build.Ssg;

/// <summary>
/// Orchestrates static site generation by discovering all routes, expanding dynamic
/// routes via <see cref="IStaticPathsProvider"/>, rendering each page to HTML, and
/// writing the output files to the configured directory.
/// </summary>
/// <remarks>
/// <para>
/// The SSG pipeline:
/// </para>
/// <list type="number">
/// <item>Enumerates all routes (static + dynamic via GetStaticPaths)</item>
/// <item>Optionally cleans the output directory</item>
/// <item>Renders each page in parallel (configurable concurrency)</item>
/// <item>Writes HTML files using clean URL conventions (<c>/about</c> → <c>about/index.html</c>)</item>
/// <item>Returns an <see cref="SsgResult"/> with per-page results and timing</item>
/// </list>
/// </remarks>
public sealed class StaticSiteGenerator
{
    private readonly SsgOptions _options;
    private readonly RouteEnumerator _routeEnumerator;
    private readonly OutputWriter _outputWriter;
    private readonly IReadOnlyDictionary<string, object?> _serviceProps;

    /// <summary>
    /// Initializes a new <see cref="StaticSiteGenerator"/> with the specified options.
    /// </summary>
    /// <param name="options">The SSG configuration options.</param>
    public StaticSiteGenerator(SsgOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _routeEnumerator = new RouteEnumerator();
        _outputWriter = new OutputWriter(options.OutputDirectory);
        _serviceProps = EmptyServiceProps;
    }

    /// <summary>
    /// Initializes a new <see cref="StaticSiteGenerator"/> with the specified options
    /// and service props for dependency injection into page components.
    /// </summary>
    /// <param name="options">The SSG configuration options.</param>
    /// <param name="serviceProps">
    /// Service props to inject into <see cref="IStaticPathsProvider"/> page components
    /// during dynamic route expansion (e.g., CollectionQuery).
    /// These props are also merged into the rendering props for each page.
    /// </param>
    public StaticSiteGenerator(SsgOptions options, IReadOnlyDictionary<string, object?> serviceProps)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(serviceProps);
        _options = options;
        _routeEnumerator = new RouteEnumerator(serviceProps);
        _outputWriter = new OutputWriter(options.OutputDirectory);
        _serviceProps = serviceProps;
    }

    /// <summary>
    /// Initializes a new <see cref="StaticSiteGenerator"/> with the specified options,
    /// route enumerator, and output writer. Used for testing.
    /// </summary>
    /// <param name="options">The SSG configuration options.</param>
    /// <param name="routeEnumerator">The route enumerator to use.</param>
    /// <param name="outputWriter">The output writer to use.</param>
    public StaticSiteGenerator(SsgOptions options, RouteEnumerator routeEnumerator, OutputWriter outputWriter)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(routeEnumerator);
        ArgumentNullException.ThrowIfNull(outputWriter);
        _options = options;
        _routeEnumerator = routeEnumerator;
        _outputWriter = outputWriter;
        _serviceProps = EmptyServiceProps;
    }

    /// <summary>
    /// Generates a static site from the specified route entries.
    /// </summary>
    /// <param name="routes">The route entries to generate pages from.</param>
    /// <param name="cancellationToken">A token to cancel the generation operation.</param>
    /// <returns>An <see cref="SsgResult"/> with per-page results and timing.</returns>
    public Task<SsgResult> GenerateAsync(IEnumerable<RouteEntry> routes, CancellationToken cancellationToken)
    {
        return GenerateAsync(routes, previousCache: null, currentAssemblyHash: "", currentContentHash: "", cancellationToken);
    }

    /// <summary>
    /// Generates a static site from the specified route entries, using the provided
    /// incremental build cache to skip pages whose inputs have not changed.
    /// </summary>
    /// <param name="routes">The route entries to generate pages from.</param>
    /// <param name="previousCache">
    /// The cache from a previous build, or <c>null</c> to force a full rebuild.
    /// </param>
    /// <param name="currentAssemblyHash">
    /// The hash of the current compiled assembly DLL.
    /// If this differs from <paramref name="previousCache"/>.<see cref="BuildCache.AssemblyHash"/>,
    /// all pages are invalidated and re-rendered.
    /// </param>
    /// <param name="currentContentHash">
    /// The hash of the current content directory (e.g., markdown files).
    /// Dynamic pages (<see cref="IStaticPathsProvider"/>) are invalidated when this changes.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the generation operation.</param>
    /// <returns>An <see cref="SsgResult"/> with per-page results and timing.</returns>
    internal async Task<SsgResult> GenerateAsync(
        IEnumerable<RouteEntry> routes,
        BuildCache? previousCache,
        string currentAssemblyHash,
        string currentContentHash,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(currentAssemblyHash);
        ArgumentNullException.ThrowIfNull(currentContentHash);

        var totalStopwatch = Stopwatch.StartNew();

        // Enumerate all concrete routes (expand dynamic routes via GetStaticPaths)
        var ssgRoutes = await _routeEnumerator.EnumerateAsync(routes);

        // Determine whether the assembly is unchanged — the primary cache invalidation signal.
        // When the assembly changes, all pages must be re-rendered regardless of content.
        var assemblyUnchanged = previousCache is not null
            && currentAssemblyHash.Length > 0
            && currentAssemblyHash == previousCache.AssemblyHash;

        // Clean output directory if configured, unless we are running incrementally
        // (assembly unchanged → output files from the previous build are still valid).
        if (_options.CleanOutputDirectory && !assemblyUnchanged)
        {
            _outputWriter.Clean();
        }

        // Render all pages (with optional per-page cache checks)
        var pageResults = await RenderAllPagesAsync(
            ssgRoutes,
            assemblyUnchanged ? previousCache : null,
            currentContentHash,
            cancellationToken);

        totalStopwatch.Stop();
        return new SsgResult(pageResults, totalStopwatch.Elapsed);
    }

    /// <summary>
    /// Renders all pages, optionally in parallel based on configuration.
    /// </summary>
    private async Task<IReadOnlyList<SsgPageResult>> RenderAllPagesAsync(
        IReadOnlyList<SsgRoute> routes,
        BuildCache? cache,
        string currentContentHash,
        CancellationToken cancellationToken)
    {
        if (routes.Count == 0)
        {
            return [];
        }

        var maxConcurrency = _options.MaxConcurrency;
        if (maxConcurrency == 1)
        {
            return await RenderSequentialAsync(routes, cache, currentContentHash, cancellationToken);
        }

        return await RenderParallelAsync(routes, maxConcurrency, cache, currentContentHash, cancellationToken);
    }

    private async Task<IReadOnlyList<SsgPageResult>> RenderSequentialAsync(
        IReadOnlyList<SsgRoute> routes,
        BuildCache? cache,
        string currentContentHash,
        CancellationToken cancellationToken)
    {
        var results = new List<SsgPageResult>(routes.Count);
        foreach (var route in routes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await RenderSinglePageAsync(route, cache, currentContentHash, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    private async Task<IReadOnlyList<SsgPageResult>> RenderParallelAsync(
        IReadOnlyList<SsgRoute> routes,
        int maxConcurrency,
        BuildCache? cache,
        string currentContentHash,
        CancellationToken cancellationToken)
    {
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
        };
        if (maxConcurrency > 0)
        {
            parallelOptions.MaxDegreeOfParallelism = maxConcurrency;
        }

        var results = new SsgPageResult[routes.Count];

        await Parallel.ForEachAsync(
            Enumerable.Range(0, routes.Count),
            parallelOptions,
            async (index, ct) =>
            {
                results[index] = await RenderSinglePageAsync(routes[index], cache, currentContentHash, ct);
            });

        return results;
    }

    /// <summary>
    /// Renders a single page and writes it to the output directory.
    /// If a valid cache entry exists and inputs are unchanged, the page is skipped.
    /// </summary>
    private async Task<SsgPageResult> RenderSinglePageAsync(
        SsgRoute route,
        BuildCache? cache,
        string currentContentHash,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // Check whether this page can be skipped based on the incremental cache.
        if (cache is not null && cache.Pages.TryGetValue(route.UrlPath, out var cachedPage))
        {
            var isDynamic = InputHasher.IsDynamicRoute(route.ComponentType);
            var contentUnchanged = !isDynamic || currentContentHash == cache.ContentHash;

            if (contentUnchanged && cachedPage.OutputPath.Length > 0 && File.Exists(cachedPage.OutputPath))
            {
                stopwatch.Stop();
                return new SsgPageResult(route, cachedPage.OutputPath, stopwatch.Elapsed);
            }
        }

        try
        {
            var html = await RenderPageToHtmlAsync(route);
            var outputPath = await _outputWriter.WritePageAsync(route.UrlPath, html, cancellationToken);

            stopwatch.Stop();
            return new SsgPageResult(route, outputPath, html, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new SsgPageResult(route, ex, stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Renders a page component to HTML, including layout wrapping and DOCTYPE injection.
    /// </summary>
    private async Task<string> RenderPageToHtmlAsync(SsgRoute route)
    {
        var componentType = route.ComponentType;
        var props = BuildProps(route);

        // Build a component delegate that renders the page inside its layout chain
        ComponentDelegate renderDelegate = async context =>
        {
            // Create the component instance — supports both IAtollComponent and IRazorSliceProxy types
            IAtollComponent component;
            if (typeof(IAtollComponent).IsAssignableFrom(componentType))
            {
                component = (IAtollComponent)Activator.CreateInstance(componentType)!;
            }
            else if (typeof(IRazorSliceProxy).IsAssignableFrom(componentType))
            {
                // Razor slice pages that don't inherit IAtollComponent are wrapped via SliceComponentAdapter.
                // The proxy is NOT a RazorSlice subtype — call CreateSlice() via the interface map.
                var interfaceMap = componentType.GetInterfaceMap(typeof(IRazorSliceProxy));
                var createSliceMethod = interfaceMap.TargetMethods
                    .FirstOrDefault(m => m.Name.EndsWith("CreateSlice", StringComparison.Ordinal)
                                      && m.GetParameters().Length == 0);
                if (createSliceMethod is null)
                {
                    throw new InvalidOperationException(
                        $"Could not find CreateSlice() method on '{componentType.FullName}'.");
                }

                var slice = (RazorSlice)createSliceMethod.Invoke(null, null)!;
                component = new SliceComponentAdapter(slice);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Component type '{componentType.FullName}' does not implement IAtollComponent or IRazorSliceProxy.");
            }

            // Render the page component to a fragment
            var pageFragment = RenderFragment.FromAsync(async destination =>
            {
                await ComponentRenderer.RenderComponentAsync(component, destination, props);
            });

            // Wrap with layouts (if any are declared via [Layout] attribute).
            // Pass props so layouts can receive service props (e.g., CollectionQuery).
            var wrappedFragment = LayoutResolver.WrapWithLayouts(componentType, pageFragment, props);

            // Render the (possibly layout-wrapped) fragment to the context's destination
            await context.RenderAsync(wrappedFragment);
        };

        // Render through PageRenderer for DOCTYPE/head injection
        var renderer = new PageRenderer();
        var result = await renderer.RenderPageAsync(renderDelegate);

        return result.Html;
    }

    /// <summary>
    /// Builds a combined props dictionary from service props, route parameters,
    /// and static path props. Service props are added first, then route parameters,
    /// then static path props (later entries override earlier ones).
    /// </summary>
    private IReadOnlyDictionary<string, object?> BuildProps(SsgRoute route)
    {
        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        // Service props first (lowest priority — overridden by route-specific props)
        foreach (var kvp in _serviceProps)
        {
            props[kvp.Key] = kvp.Value;
        }

        // Add route parameters
        foreach (var kvp in route.Parameters)
        {
            props[kvp.Key] = kvp.Value;
        }

        // Static path props override route parameters
        foreach (var kvp in route.Props)
        {
            props[kvp.Key] = kvp.Value;
        }

        return new ReadOnlyDictionary<string, object?>(props);
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyServiceProps =
        new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
}

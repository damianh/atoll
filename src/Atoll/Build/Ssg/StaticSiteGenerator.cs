using System.Collections.ObjectModel;
using System.Diagnostics;
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;

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
    /// <returns>An <see cref="SsgResult"/> with per-page results and timing.</returns>
    public async Task<SsgResult> GenerateAsync(IEnumerable<RouteEntry> routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        var totalStopwatch = Stopwatch.StartNew();

        // Enumerate all concrete routes (expand dynamic routes via GetStaticPaths)
        var ssgRoutes = await _routeEnumerator.EnumerateAsync(routes);

        // Clean output directory if configured
        if (_options.CleanOutputDirectory)
        {
            _outputWriter.Clean();
        }

        // Render all pages
        var pageResults = await RenderAllPagesAsync(ssgRoutes);

        totalStopwatch.Stop();
        return new SsgResult(pageResults, totalStopwatch.Elapsed);
    }

    /// <summary>
    /// Renders all pages, optionally in parallel based on configuration.
    /// </summary>
    private async Task<IReadOnlyList<SsgPageResult>> RenderAllPagesAsync(IReadOnlyList<SsgRoute> routes)
    {
        if (routes.Count == 0)
        {
            return [];
        }

        var maxConcurrency = _options.MaxConcurrency;
        if (maxConcurrency == 1)
        {
            // Sequential rendering
            return await RenderSequentialAsync(routes);
        }

        // Parallel rendering
        return await RenderParallelAsync(routes, maxConcurrency);
    }

    private async Task<IReadOnlyList<SsgPageResult>> RenderSequentialAsync(IReadOnlyList<SsgRoute> routes)
    {
        var results = new List<SsgPageResult>(routes.Count);
        foreach (var route in routes)
        {
            var result = await RenderSinglePageAsync(route);
            results.Add(result);
        }

        return results;
    }

    private async Task<IReadOnlyList<SsgPageResult>> RenderParallelAsync(
        IReadOnlyList<SsgRoute> routes,
        int maxConcurrency)
    {
        var parallelOptions = new ParallelOptions();
        if (maxConcurrency > 0)
        {
            parallelOptions.MaxDegreeOfParallelism = maxConcurrency;
        }

        var results = new SsgPageResult[routes.Count];

        await Parallel.ForEachAsync(
            Enumerable.Range(0, routes.Count),
            parallelOptions,
            async (index, cancellationToken) =>
            {
                results[index] = await RenderSinglePageAsync(routes[index]);
            });

        return results;
    }

    /// <summary>
    /// Renders a single page and writes it to the output directory.
    /// </summary>
    private async Task<SsgPageResult> RenderSinglePageAsync(SsgRoute route)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var html = await RenderPageToHtmlAsync(route);
            var outputPath = await _outputWriter.WritePageAsync(route.UrlPath, html);

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
            var component = (IAtollComponent)Activator.CreateInstance(componentType)!;

            // Render the page component to a fragment
            var pageFragment = RenderFragment.FromAsync(async destination =>
            {
                await ComponentRenderer.RenderComponentAsync(component, destination, props);
            });

            // Wrap with layouts (if any are declared via [Layout] attribute)
            var wrappedFragment = LayoutResolver.WrapWithLayouts(componentType, pageFragment);

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
        new Dictionary<string, object?>();
}

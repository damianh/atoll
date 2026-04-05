using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Atoll.Build.Content.Collections;
using Atoll.Configuration;
using Atoll.Css;
using Atoll.Islands;
using Atoll.Middleware.Server.Hosting;
using Atoll.Routing;
using Atoll.Routing.FileSystem;
using Atoll.Routing.Matching;
using Microsoft.Extensions.Logging;

namespace Atoll.Cli.Commands.Dev;

/// <summary>
/// Orchestrates the build → load → discover → state-swap cycle for
/// <c>atoll dev</c> hot-reload. Builds an initial <see cref="DevServerState"/>
/// and can produce updated states on code or content changes.
/// </summary>
internal sealed class DevServerReloader
{
    private readonly string _projectRoot;
    private readonly string _csprojPath;
    private readonly ILogger<DevServerReloader> _logger;
    private int _alcCounter;

    /// <summary>
    /// Initializes a new <see cref="DevServerReloader"/>.
    /// </summary>
    /// <param name="projectRoot">The project root directory (contains <c>atoll.json</c>).</param>
    /// <param name="csprojPath">Absolute path to the <c>.csproj</c> file.</param>
    /// <param name="logger">The logger instance.</param>
    public DevServerReloader(
        string projectRoot,
        string csprojPath,
        ILogger<DevServerReloader> logger)
    {
        ArgumentNullException.ThrowIfNull(projectRoot);
        ArgumentNullException.ThrowIfNull(csprojPath);
        ArgumentNullException.ThrowIfNull(logger);
        _projectRoot = projectRoot;
        _csprojPath = csprojPath;
        _logger = logger;
    }

    /// <summary>
    /// Builds the initial <see cref="DevServerState"/> by compiling the project,
    /// loading the assembly, discovering routes, and building the content query.
    /// Returns the state and any build error output (null on success).
    /// </summary>
    public async Task<(DevServerState State, string? BuildError)> BuildInitialStateAsync()
    {
        return await BuildStateAsync(currentLoadContext: null, currentAssembly: null, currentState: null);
    }

    /// <summary>
    /// Produces a new <see cref="DevServerState"/> in response to a detected file change.
    /// Returns the state and any build error output (null on success).
    /// </summary>
    /// <param name="current">The current state (used for content-only reloads and as fallback on failure).</param>
    /// <param name="changeKind">The kind of change that was detected.</param>
    public async Task<(DevServerState State, string? BuildError)> ReloadAsync(DevServerState current, FileChangeKind changeKind)
    {
        ArgumentNullException.ThrowIfNull(current);

        if (changeKind == FileChangeKind.ContentOnly)
        {
            return (await ReloadContentOnlyAsync(current), null);
        }

        return await BuildStateAsync(current.LoadContext, current.UserAssembly, current);
    }

    // ── Private: full code-change rebuild ──────────────────────────────────────

    private async Task<(DevServerState State, string? BuildError)> BuildStateAsync(
        AssemblyLoadContext? currentLoadContext,
        Assembly? currentAssembly,
        DevServerState? currentState)
    {
        var sw = Stopwatch.StartNew();
        Console.WriteLine($"  Building {Path.GetFileName(_csprojPath)}...");

        // Re-read atoll.json on every code-change build so that configuration
        // changes (e.g. Src directory, SiteUrl) are picked up without a restart.
        var config = await AtollConfigLoader.LoadAsync(_projectRoot);
        var pagesDirectory = AtollConfigLoader.ResolveSrcDirectory(config, _projectRoot);

        var buildResult = await BuildProjectAsync(_csprojPath);
        if (!buildResult.Success)
        {
            Console.WriteLine("  Warning: Build failed — keeping current state.");
            // Preserve the current working state so the browser keeps showing content.
            var fallbackState = currentState ?? BuildEmptyState();
            return (fallbackState, buildResult.ErrorOutput);
        }

        var assemblyPath = FindOutputAssembly(_csprojPath);
        if (assemblyPath is null)
        {
            Console.WriteLine("  Warning: Could not locate compiled assembly.");
            return (currentState ?? BuildEmptyState(), "Could not locate compiled assembly after successful build.");
        }

        var counter = System.Threading.Interlocked.Increment(ref _alcCounter);
        var (loadContext, assembly) = LoadAssembly(assemblyPath, $"AtollDev-{counter}");
        if (assembly is null)
        {
            return (currentState ?? BuildEmptyState(), "Failed to load compiled assembly.");
        }

        var routes = DiscoverRoutes(assembly, pagesDirectory);
        var collectionQuery = CreateCollectionQueryFromAssembly(assembly, _projectRoot);
        var options = BuildOptions(collectionQuery);
        var globalCss = AggregateGlobalCss(assembly);
        var islandAssets = DiscoverIslandAssets(assembly);
        var searchIndex = collectionQuery is not null
            ? GenerateSearchIndexBytes(assembly, collectionQuery)
            : null;

        Console.WriteLine($"  Routes: {routes.Count} discovered");
        Console.WriteLine($"  Islands: {islandAssets.Count} JS asset(s) loaded");
        Console.WriteLine($"  Reload complete ({sw.ElapsedMilliseconds}ms)");

        return (new DevServerState(new RouteMatcher(routes), options, loadContext, assembly, globalCss, islandAssets, searchIndex), null);
    }

    // ── Private: content-only reload ───────────────────────────────────────────

    private Task<DevServerState> ReloadContentOnlyAsync(DevServerState current)
    {
        Console.WriteLine("  Reloading content...");
        var sw = Stopwatch.StartNew();

        // Reuse the existing assembly and ALC — only rebuild CollectionQuery.
        var collectionQuery = current.UserAssembly is not null
            ? CreateCollectionQueryFromAssembly(current.UserAssembly, _projectRoot)
            : null;

        var options = BuildOptions(collectionQuery);

        // Preserve existing routes — code hasn't changed.
        Console.WriteLine($"  Reload complete ({sw.ElapsedMilliseconds}ms)");

        // Regenerate search index from updated content but reuse island assets (code unchanged).
        var searchIndex = collectionQuery is not null && current.UserAssembly is not null
            ? GenerateSearchIndexBytes(current.UserAssembly, collectionQuery)
            : current.SearchIndexJson;

        return Task.FromResult(
            new DevServerState(current.RouteMatcher, options, current.LoadContext, current.UserAssembly, current.GlobalCss, current.IslandAssets, searchIndex));
    }

    // ── Private: shared helpers ─────────────────────────────────────────────────

    private static DevServerState BuildEmptyState()
    {
        var options = new AtollOptions();
        return new DevServerState(new RouteMatcher([]), options, null, null, "", new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>()), null);
    }

    private static AtollOptions BuildOptions(CollectionQuery? collectionQuery)
    {
        var options = new AtollOptions();
        if (collectionQuery is not null)
        {
            options.ServiceProps["Query"] = collectionQuery;
        }
        return options;
    }

    private IReadOnlyList<RouteEntry> DiscoverRoutes(Assembly assembly, string pagesDirectory)
    {
        var discovery = new RouteDiscovery(pagesDirectory);
        var routes = discovery.DiscoverRoutes(new[] { assembly });

        if (routes.Count == 0)
        {
            routes = DiscoverRoutesFromTypes(assembly);
        }

        return routes;
    }

    /// <summary>
    /// Discovers all <c>[GlobalStyle]</c> components from the user assembly and its
    /// referenced assemblies, aggregates their CSS, and returns the combined result.
    /// </summary>
    private string AggregateGlobalCss(Assembly userAssembly)
    {
        var assemblies = GetAssemblyWithReferences(userAssembly);
        var globalTypes = GlobalStyleDiscovery.DiscoverGlobalStyles(assemblies);

        if (globalTypes.Count == 0)
        {
            return "";
        }

        var aggregator = new CssAggregator();
        foreach (var type in globalTypes)
        {
            aggregator.Add(type);
        }

        var css = aggregator.GetCombinedCss();
        _logger.LogDebug("Discovered {Count} global style(s), {Length} chars of CSS", globalTypes.Count, css.Length);
        return css;
    }

    /// <summary>
    /// Returns the user assembly plus any referenced assemblies that are already loaded
    /// in the same <see cref="AssemblyLoadContext"/> (e.g. Atoll.Lagoon).
    /// </summary>
    private static IEnumerable<Assembly> GetAssemblyWithReferences(Assembly userAssembly)
    {
        yield return userAssembly;

        var loadContext = AssemblyLoadContext.GetLoadContext(userAssembly);
        if (loadContext is null)
        {
            yield break;
        }

        foreach (var referencedName in userAssembly.GetReferencedAssemblies())
        {
            Assembly? referenced;
            try
            {
                referenced = loadContext.LoadFromAssemblyName(referencedName);
            }
            catch
            {
                continue;
            }

            yield return referenced;
        }
    }

    /// <summary>
    /// Discovers all <see cref="IIslandAssetProvider"/> implementations from the user assembly
    /// and its referenced assemblies, reads their embedded resources into memory, and returns
    /// a dictionary keyed by URL-relative output path (no leading slash).
    /// </summary>
    private IReadOnlyDictionary<string, byte[]> DiscoverIslandAssets(Assembly userAssembly)
    {
        var assemblies = GetAssemblyWithReferences(userAssembly).ToList();
        var allAssets = new List<IslandAssetDescriptor>();

        foreach (var assembly in assemblies)
        {
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t is not null).Cast<Type>();
            }

            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (!typeof(IIslandAssetProvider).IsAssignableFrom(type))
                {
                    continue;
                }

                try
                {
                    var provider = (IIslandAssetProvider)Activator.CreateInstance(type)!;
                    allAssets.AddRange(provider.GetAssets());
                }
                catch
                {
                    // Ignore providers that cannot be instantiated.
                }
            }
        }

        var result = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var descriptor in allAssets)
        {
            using var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName);
            if (stream is null)
            {
                _logger.LogWarning(
                    "Embedded resource '{ResourceName}' not found in assembly '{Assembly}'",
                    descriptor.ResourceName,
                    descriptor.ResourceAssembly.GetName().Name);
                continue;
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            result[descriptor.OutputPath] = ms.ToArray();
        }

        _logger.LogDebug("Discovered {Count} island asset(s)", result.Count);
        return new ReadOnlyDictionary<string, byte[]>(result);
    }

    /// <summary>
    /// Generates the search index JSON bytes by invoking <c>LagoonSearchIndexGenerator</c>
    /// via reflection (mirroring <c>BuildCommandHandler.GenerateSearchIndexAsync</c> but
    /// writing to a <see cref="MemoryStream"/> instead of disk). Returns <c>null</c> if no
    /// <c>ISearchIndexConfiguration</c> implementation is found.
    /// </summary>
    private byte[]? GenerateSearchIndexBytes(Assembly userAssembly, CollectionQuery collectionQuery)
    {
        const string interfaceName = "Atoll.Lagoon.Search.ISearchIndexConfiguration";
        const string generatorTypeName = "Atoll.Lagoon.Search.LagoonSearchIndexGenerator";

        // Find ISearchIndexConfiguration implementation in the user assembly.
        Type? configType = null;
        try
        {
            foreach (var type in userAssembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (type.GetInterface(interfaceName) is not null)
                {
                    configType = type;
                    break;
                }
            }
        }
        catch (ReflectionTypeLoadException)
        {
            return null;
        }

        if (configType is null)
        {
            return null;
        }

        // Resolve LagoonSearchIndexGenerator from loaded assemblies.
        var loadContext = AssemblyLoadContext.GetLoadContext(userAssembly);
        Type? generatorType = null;

        if (loadContext is not null)
        {
            foreach (var asm in loadContext.Assemblies)
            {
                generatorType = asm.GetType(generatorTypeName);
                if (generatorType is not null)
                {
                    break;
                }
            }
        }

        if (generatorType is null)
        {
            _logger.LogDebug("ISearchIndexConfiguration found but LagoonSearchIndexGenerator could not be resolved");
            return null;
        }

        // Find the GenerateToStreamAsync(CollectionQuery, ISearchIndexConfiguration, Stream) method,
        // or fall back to GenerateAsync and capture the output differently.
        // First, try to find a stream-based generator method.
        MethodInfo? generateMethod = null;
        foreach (var method in generatorType.GetMethods())
        {
            if (method.Name != "GenerateToStreamAsync")
            {
                continue;
            }

            var parameters = method.GetParameters();
            if (parameters.Length == 3
                && parameters[0].ParameterType == typeof(CollectionQuery)
                && parameters[2].ParameterType == typeof(Stream))
            {
                generateMethod = method;
                break;
            }
        }

        try
        {
            var configInstance = Activator.CreateInstance(configType)!;

            if (generateMethod is not null)
            {
                // Use stream-based generation if available.
                using var ms = new MemoryStream();
                var generator = Activator.CreateInstance(generatorType)!;
                var task = (Task)generateMethod.Invoke(generator, [collectionQuery, configInstance, ms])!;
                task.GetAwaiter().GetResult();
                var bytes = ms.ToArray();
                _logger.LogDebug("Search index generated: {Bytes} bytes", bytes.Length);
                return bytes;
            }

            // Fall back to the file-based generator: create a temp directory, generate, read, clean up.
            var tempDir = Path.Combine(Path.GetTempPath(), "atoll-dev-search-" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(tempDir);
            try
            {
                MethodInfo? fileGenerateMethod = null;
                foreach (var method in generatorType.GetMethods())
                {
                    if (method.Name != "GenerateAsync")
                    {
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length == 2
                        && parameters[0].ParameterType == typeof(CollectionQuery))
                    {
                        fileGenerateMethod = method;
                        break;
                    }
                }

                if (fileGenerateMethod is null)
                {
                    _logger.LogDebug("No suitable GenerateAsync method found on LagoonSearchIndexGenerator");
                    return null;
                }

                var fileGenerator = Activator.CreateInstance(generatorType, tempDir)!;
                var fileTask = (Task)fileGenerateMethod.Invoke(fileGenerator, [collectionQuery, configInstance])!;
                fileTask.GetAwaiter().GetResult();

                var indexPath = Path.Combine(tempDir, "search-index.json");
                if (!File.Exists(indexPath))
                {
                    _logger.LogDebug("Search index file was not generated at expected path");
                    return null;
                }

                var bytes = File.ReadAllBytes(indexPath);
                _logger.LogDebug("Search index generated: {Bytes} bytes", bytes.Length);
                return bytes;
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); }
                catch { /* best-effort cleanup */ }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate search index");
            return null;
        }
    }

    private static IReadOnlyList<RouteEntry> DiscoverRoutesFromTypes(Assembly assembly)
    {
        var entries = new List<RouteEntry>();

        foreach (var type in assembly.GetExportedTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            if (!typeof(IAtollPage).IsAssignableFrom(type))
            {
                continue;
            }

            var routeAttr = type.GetCustomAttribute<PageRouteAttribute>();
            if (routeAttr is not null)
            {
                var pattern = routeAttr.Pattern.StartsWith('/')
                    ? routeAttr.Pattern
                    : "/" + routeAttr.Pattern;
                entries.Add(new RouteEntry(pattern, type, pattern));
                continue;
            }

            var filePath = InferFilePathFromType(type);
            var inferred = RouteDiscovery.DiscoverRoutesFromEntries(new[] { (filePath, type) });
            entries.AddRange(inferred);
        }

        return entries;
    }

    private static string InferFilePathFromType(Type type)
    {
        var name = type.Name;

        if (name.EndsWith("Page", StringComparison.Ordinal))
        {
            name = name[..^4];
        }

        if (name.Equals("Index", StringComparison.OrdinalIgnoreCase))
        {
            return "index.cs";
        }

        if (name.EndsWith("Index", StringComparison.Ordinal))
        {
            return ToKebabCase(name[..^5]) + "/index.cs";
        }

        if (typeof(IStaticPathsProvider).IsAssignableFrom(type))
        {
            return ToKebabCase(name) + "/[slug].cs";
        }

        return ToKebabCase(name) + ".cs";
    }

    private static string ToKebabCase(string pascalCase)
    {
        if (pascalCase.Length == 0)
        {
            return pascalCase;
        }

        var chars = new List<char>(pascalCase.Length + 4);
        for (var i = 0; i < pascalCase.Length; i++)
        {
            var c = pascalCase[i];
            if (char.IsUpper(c) && i > 0)
            {
                chars.Add('-');
            }
            chars.Add(char.ToLowerInvariant(c));
        }

        return new string(chars.ToArray());
    }

    private static CollectionQuery? CreateCollectionQueryFromAssembly(
        Assembly assembly,
        string projectRoot)
    {
        Type? configType = null;

        foreach (var type in assembly.GetExportedTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            if (!typeof(IContentConfiguration).IsAssignableFrom(type))
            {
                continue;
            }

            configType = type;
            break;
        }

        if (configType is null)
        {
            return null;
        }

        var configInstance = (IContentConfiguration)Activator.CreateInstance(configType)!;
        var collectionConfig = configInstance.Configure();

        var resolvedBaseDir = Path.IsPathRooted(collectionConfig.BaseDirectory)
            ? collectionConfig.BaseDirectory
            : Path.GetFullPath(Path.Combine(projectRoot, collectionConfig.BaseDirectory));

        var resolvedConfig = new CollectionConfig(resolvedBaseDir);
        foreach (var kvp in collectionConfig.Collections)
        {
            resolvedConfig.AddCollection(kvp.Value);
        }

        var fileProvider = new PhysicalFileProvider();
        var loader = new CollectionLoader(resolvedConfig, fileProvider);
        return collectionConfig.Markdown is { } markdown
            ? new CollectionQuery(loader, markdown)
            : new CollectionQuery(loader);
    }

    private static async Task<BuildResult> BuildProjectAsync(string csprojPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{csprojPath}\" -c Release --nologo -v q",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
        {
            return new BuildResult(false, "Failed to start dotnet build process.");
        }

        // Read stdout and stderr concurrently before WaitForExitAsync to avoid
        // deadlocks when the process fills its output buffer.
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode == 0)
        {
            return new BuildResult(true, null);
        }

        var errorOutput = string.Join(Environment.NewLine, stdout, stderr).Trim();
        return new BuildResult(false, errorOutput.Length > 0 ? errorOutput : null);
    }

    private static string? FindOutputAssembly(string csprojPath)
    {
        var projectDir = Path.GetDirectoryName(csprojPath)!;
        var projectName = Path.GetFileNameWithoutExtension(csprojPath);

        var candidates = new[]
        {
            Path.Combine(projectDir, "bin", "Release"),
            Path.Combine(projectDir, "bin", "Debug"),
        };

        foreach (var binDir in candidates)
        {
            if (!Directory.Exists(binDir))
            {
                continue;
            }

            var tfmDirs = Directory.GetDirectories(binDir);
            foreach (var tfmDir in tfmDirs)
            {
                var dllPath = Path.Combine(tfmDir, projectName + ".dll");
                if (File.Exists(dllPath))
                {
                    return dllPath;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Loads an assembly using a new collectible <see cref="AssemblyLoadContext"/>.
    /// Collectible contexts allow the old assembly to be garbage-collected after
    /// hot-reload. Returns <c>(null, null)</c> on failure.
    /// </summary>
    private (AssemblyLoadContext? Context, Assembly? Assembly) LoadAssembly(
        string assemblyPath,
        string contextName)
    {
        try
        {
            var absolutePath = Path.GetFullPath(assemblyPath);

            // isCollectible: true — allows this ALC to be unloaded after hot-reload,
            // preventing memory leaks from accumulated assembly loads.
            var loadContext = new AssemblyLoadContext(contextName, isCollectible: true);

            // Resolve transitive NuGet dependencies (e.g. TextMateSharp) by
            // parsing the user project's .deps.json and probing the NuGet cache.
            // AssemblyDependencyResolver does not work for class library projects
            // (no .runtimeconfig.json), so we use our own resolver.
            var resolver = DepsJsonAssemblyResolver.Create(absolutePath);
            resolver?.Attach(loadContext);

            var assembly = loadContext.LoadFromAssemblyPath(absolutePath);
            return (loadContext, assembly);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load assembly '{Path}'", assemblyPath);
            Console.WriteLine($"  Warning: Failed to load assembly: {ex.Message}");
            return (null, null);
        }
    }
}

/// <summary>
/// Represents the result of a <c>dotnet build</c> invocation.
/// </summary>
/// <param name="Success">Whether the build succeeded.</param>
/// <param name="ErrorOutput">The combined stdout and stderr output on failure; <c>null</c> on success.</param>
internal sealed record BuildResult(bool Success, string? ErrorOutput);

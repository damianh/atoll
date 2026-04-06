using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Atoll.Build.Content.Collections;
using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;
using Atoll.Configuration;
using Atoll.Css;
using Atoll.Islands;
using Atoll.Redirects;
using Atoll.Routing;
using Atoll.Routing.FileSystem;

namespace Atoll.Cli.Commands;

/// <summary>
/// Handles the <c>atoll build</c> command. Runs the full SSG pipeline:
/// load config → build project → discover routes → render pages → process assets → post-process HTML → write manifest.
/// </summary>
public sealed class BuildCommandHandler
{
    /// <summary>
    /// Executes the build command.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    public async Task ExecuteAsync(string projectRoot)
    {
        var startTime = DateTime.UtcNow;
        Console.WriteLine("Atoll — building site...");

        // Load configuration
        var config = await AtollConfigLoader.LoadAsync(projectRoot);
        var outputDir = AtollConfigLoader.ResolveOutputDirectory(config, projectRoot);
        var publicDir = AtollConfigLoader.ResolvePublicDirectory(config, projectRoot);
        var srcDir = AtollConfigLoader.ResolveSrcDirectory(config, projectRoot);
        var basePath = AtollConfigLoader.NormalizeBasePath(config.Base);
        var basePathForAssets = basePath == "/" ? "" : basePath;

        Console.WriteLine($"  Output: {outputDir}");
        Console.WriteLine($"  Site:   {(config.Site.Length > 0 ? config.Site : "(not configured)")}");
        if (basePath != "/")
        {
            Console.WriteLine($"  Base:   {basePath}");
        }

        // Build the user project and discover routes
        var (routes, assembly) = await BuildAndDiscoverRoutesAsync(projectRoot, srcDir);

        Console.WriteLine($"  Routes: {routes.Count} discovered");

        // Build service props (e.g., CollectionQuery for content-driven pages)
        var serviceProps = BuildServiceProps(assembly, projectRoot);

        // SSG options
        var ssgOptions = new SsgOptions(outputDir)
        {
            BaseUrl = config.Site,
            BasePath = basePathForAssets,
            MaxConcurrency = config.Build.Concurrency,
            CleanOutputDirectory = config.Build.Clean,
        };

        // Generate static site
        var generator = new StaticSiteGenerator(ssgOptions, serviceProps);
        var ssgResult = await generator.GenerateAsync(routes);

        // Asset pipeline
        var pipelineOptions = new AssetPipelineOptions(outputDir)
        {
            BasePath = basePathForAssets,
            PublicDirectory = Directory.Exists(publicDir) ? publicDir : "",
            Minify = config.Build.Minify,
            Fingerprint = config.Build.Fingerprint,
        };

        var outputWriter = new OutputWriter(outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);

        // Discover global CSS types from the user assembly and its referenced assemblies
        var globalStyleTypes = assembly is not null
            ? DiscoverGlobalStyleTypes(assembly)
            : Array.Empty<Type>();

        if (globalStyleTypes.Count > 0)
        {
            Console.WriteLine($"  CSS:     {globalStyleTypes.Count} global style type(s) discovered");
        }

        var assetResult = await pipeline.RunAsync(globalStyleTypes, Array.Empty<string>());

        // Write island JS assets from library providers
        if (assembly is not null)
        {
            await WriteIslandAssetsAsync(assembly, outputDir);
        }

        // Write core framework assets (_atoll/island.js, _atoll/directives.js, _atoll/logo.png)
        await WriteCoreFrameworkAssetsAsync(assembly, outputDir);

        // Post-process HTML
        var cssHref = assetResult.Css.HasContent
            ? "/" + assetResult.Css.OutputPath.Replace('\\', '/')
            : "";
        var jsHref = assetResult.Js.HasContent
            ? "/" + assetResult.Js.OutputPath.Replace('\\', '/')
            : "";

        if (cssHref.Length > 0 || jsHref.Length > 0)
        {
            var postProcessorOptions = new HtmlPostProcessorOptions
            {
                CssHref = cssHref,
                JsHref = jsHref,
                BasePath = basePathForAssets,
                RemoveInlineStyles = true,
            };
            var postProcessor = new HtmlPostProcessor(postProcessorOptions);

            foreach (var pageResult in ssgResult.PageResults)
            {
                if (!pageResult.IsSuccess || pageResult.OutputPath.Length == 0)
                {
                    continue;
                }

                var processedHtml = postProcessor.Process(pageResult.Html);
                await File.WriteAllTextAsync(pageResult.OutputPath, processedHtml);
            }
        }

        // Write build manifest
        var manifestWriter = new BuildManifestWriter(outputDir);
        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, ssgOptions);
        await manifestWriter.WriteAsync(manifest);

        // Write redirects.json for SSG deployments.
        // The redirect map is populated by the user site via RedirectCollector.Collect().
        // When the loaded assembly exposes an IRedirectMapProvider, use it;
        // otherwise write an empty redirects.json as a stable artifact.
        var redirectMap = BuildRedirectMapFromAssembly(assembly);
        var redirectsWriter = new RedirectsFileWriter(outputDir);
        await redirectsWriter.WriteAsync(redirectMap);

        // Generate _headers file for Netlify / Cloudflare Pages deployments
        if (config.Build.Cache.GenerateHeadersFile)
        {
            var headersGenerator = new HeadersFileGenerator(config.Build.Cache);
            await headersGenerator.WriteAsync(outputDir);
            Console.WriteLine("  Headers: _headers file generated");
        }

        // Generate search index (if project implements ISearchIndexConfiguration from Atoll.Lagoon)
        if (assembly is not null && serviceProps.TryGetValue("Query", out var queryObj) && queryObj is CollectionQuery collectionQuery)
        {
            await GenerateSearchIndexAsync(assembly, collectionQuery, outputDir);
            await ValidateLinksAsync(assembly, collectionQuery);
            await GenerateRedirectsAsync(assembly, collectionQuery, outputDir);
            await GenerateOgImagesAsync(assembly, collectionQuery, outputDir, projectRoot);
        }

        // Report results
        var elapsed = DateTime.UtcNow - startTime;
        Console.WriteLine();
        Console.WriteLine($"  Pages:  {ssgResult.SuccessCount} rendered");
        if (ssgResult.FailureCount > 0)
        {
            Console.WriteLine($"  Errors: {ssgResult.FailureCount} failed");
            foreach (var failure in ssgResult.Failures)
            {
                Console.WriteLine($"    ✗ {failure.Route.UrlPath}: {failure.Error?.Message}");
            }
        }

        if (assetResult.StaticAssets is not null)
        {
            Console.WriteLine($"  Assets: {assetResult.StaticAssets.Count} static files copied");
        }

        Console.WriteLine($"  Time:   {elapsed.TotalMilliseconds:F0}ms");
        Console.WriteLine($"  Done!   {outputDir}");
    }

    /// <summary>
    /// Builds service props by discovering content configuration from the loaded assembly.
    /// If the assembly implements <see cref="IContentConfiguration"/>, a <see cref="CollectionQuery"/>
    /// is created and included as a service prop for injection into page components.
    /// </summary>
    private static IReadOnlyDictionary<string, object?> BuildServiceProps(
        Assembly? assembly,
        string projectRoot)
    {
        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (assembly is null)
        {
            return props;
        }

        var collectionQuery = CreateCollectionQueryFromAssembly(assembly, projectRoot);
        if (collectionQuery is not null)
        {
            props["Query"] = collectionQuery;
            Console.WriteLine("  Content: collection configuration discovered");
        }

        return props;
    }

    /// <summary>
    /// Scans the assembly for an <see cref="IContentConfiguration"/> implementation
    /// and creates a <see cref="CollectionQuery"/> from it.
    /// </summary>
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

        // Resolve the base directory relative to the project root
        var resolvedBaseDir = Path.IsPathRooted(collectionConfig.BaseDirectory)
            ? collectionConfig.BaseDirectory
            : Path.GetFullPath(Path.Combine(projectRoot, collectionConfig.BaseDirectory));

        // Create a new config with the resolved absolute path
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

    /// <summary>
    /// Builds the user project and discovers routable types from the compiled assembly.
    /// Returns the discovered routes and the loaded assembly (for further scanning).
    /// </summary>
    private static async Task<(IReadOnlyList<RouteEntry> Routes, Assembly? Assembly)> BuildAndDiscoverRoutesAsync(
        string projectRoot,
        string pagesDirectory)
    {
        var csprojPath = FindProjectFile(projectRoot);
        if (csprojPath is null)
        {
            Console.WriteLine("  Warning: No .csproj found — no routes to discover.");
            return ([], null);
        }

        Console.WriteLine($"  Building {Path.GetFileName(csprojPath)}...");

        // Build the project
        var buildSuccess = await BuildProjectAsync(csprojPath);
        if (!buildSuccess)
        {
            Console.WriteLine("  Error: Project build failed. Cannot discover routes.");
            return ([], null);
        }

        // Find and load the assembly
        var assemblyPath = FindOutputAssembly(csprojPath);
        if (assemblyPath is null)
        {
            Console.WriteLine("  Warning: Could not locate compiled assembly — no routes to discover.");
            return ([], null);
        }

        var assembly = LoadAssembly(assemblyPath);
        if (assembly is null)
        {
            Console.WriteLine("  Warning: Failed to load assembly — no routes to discover.");
            return ([], null);
        }

        // Discover routes using assembly scanning
        var discovery = new RouteDiscovery(pagesDirectory);
        var routes = discovery.DiscoverRoutes(new[] { assembly });

        // If file-based discovery found nothing (no Pages/ directory on disk),
        // fall back to discovering from type metadata directly
        if (routes.Count == 0)
        {
            routes = DiscoverRoutesFromTypes(assembly);
        }

        return (routes, assembly);
    }

    /// <summary>
    /// Discovers global CSS component types from the assembly and its referenced assemblies.
    /// Scans the assembly's output directory for referenced DLLs and includes types decorated
    /// with both <see cref="GlobalStyleAttribute"/> and <see cref="StylesAttribute"/>.
    /// </summary>
    private static IReadOnlyList<Type> DiscoverGlobalStyleTypes(Assembly assembly)
    {
        var assembliesToScan = new List<Assembly> { assembly };
        var assemblyDir = Path.GetDirectoryName(assembly.Location);

        if (assemblyDir is not null)
        {
            foreach (var referencedName in assembly.GetReferencedAssemblies())
            {
                var candidatePath = Path.Combine(assemblyDir, referencedName.Name + ".dll");
                if (!File.Exists(candidatePath))
                {
                    continue;
                }

                try
                {
                    var referenced = Assembly.LoadFrom(candidatePath);
                    assembliesToScan.Add(referenced);
                }
                catch
                {
                    // Ignore load failures for individual assemblies
                }
            }
        }

        return GlobalStyleDiscovery.DiscoverGlobalStyles(assembliesToScan);
    }

    /// <summary>
    /// Discovers <see cref="IIslandAssetProvider"/> implementations from the assembly and its
    /// referenced assemblies, then writes all declared island JS assets to the output directory.
    /// </summary>
    private static async Task WriteIslandAssetsAsync(Assembly assembly, string outputDirectory)
    {
        var assembliesToScan = new List<Assembly> { assembly };
        var assemblyDir = Path.GetDirectoryName(assembly.Location);

        if (assemblyDir is not null)
        {
            foreach (var referencedName in assembly.GetReferencedAssemblies())
            {
                var candidatePath = Path.Combine(assemblyDir, referencedName.Name + ".dll");
                if (!File.Exists(candidatePath))
                {
                    continue;
                }

                try
                {
                    var referenced = Assembly.LoadFrom(candidatePath);
                    assembliesToScan.Add(referenced);
                }
                catch
                {
                    // Ignore load failures for individual assemblies
                }
            }
        }

        var allAssets = new List<IslandAssetDescriptor>();

        foreach (var scannedAssembly in assembliesToScan)
        {
            IEnumerable<Type> types;
            try
            {
                types = scannedAssembly.GetTypes();
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
                    // Ignore providers that cannot be instantiated
                }
            }
        }

        if (allAssets.Count == 0)
        {
            return;
        }

        var writer = new IslandAssetWriter(outputDirectory);
        var writeResult = await writer.WriteAsync(allAssets);
        Console.WriteLine($"  Islands: {writeResult.FileCount} JS asset(s) written");
    }

    /// <summary>
    /// Validates internal links if the user's assembly implements
    /// <c>Atoll.Lagoon.Validation.ILinkValidationConfiguration</c>.
    /// Uses reflection to avoid a hard dependency on <c>Atoll.Lagoon</c>.
    /// </summary>
    private static async Task ValidateLinksAsync(
        Assembly assembly,
        CollectionQuery collectionQuery)
    {
        const string interfaceName = "Atoll.Lagoon.Validation.ILinkValidationConfiguration";
        const string validatorTypeName = "Atoll.Lagoon.Validation.LagoonLinkValidator";

        // Find an ILinkValidationConfiguration implementation in the user's assembly
        Type? configType = null;
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            var iface = type.GetInterface(interfaceName);
            if (iface is not null)
            {
                configType = type;
                break;
            }
        }

        if (configType is null)
        {
            return;
        }

        // Resolve LagoonLinkValidator from the assembly's dependencies
        Type? validatorType = null;
        foreach (var referencedAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            validatorType = referencedAssembly.GetType(validatorTypeName);
            if (validatorType is not null)
            {
                break;
            }
        }

        // Also try to find it via the assembly directory
        if (validatorType is null)
        {
            var assemblyDir = Path.GetDirectoryName(assembly.Location);
            if (assemblyDir is not null)
            {
                var lagoonDll = Path.Combine(assemblyDir, "Atoll.Lagoon.dll");
                if (File.Exists(lagoonDll))
                {
                    try
                    {
                        var lagoonAssembly = Assembly.LoadFrom(lagoonDll);
                        validatorType = lagoonAssembly.GetType(validatorTypeName);
                    }
                    catch
                    {
                        // Ignore load failures
                    }
                }
            }
        }

        if (validatorType is null)
        {
            Console.WriteLine("  Warning: ILinkValidationConfiguration found but Atoll.Lagoon assembly could not be loaded.");
            return;
        }

        // Create config and validator instances
        var configInstance = Activator.CreateInstance(configType)!;
        var validator = Activator.CreateInstance(validatorType)!;

        // Find Validate(CollectionQuery, ILinkValidationConfiguration) overload
        MethodInfo? validateMethod = null;
        foreach (var method in validatorType.GetMethods())
        {
            if (method.Name != "Validate")
            {
                continue;
            }

            var parameters = method.GetParameters();
            if (parameters.Length == 2
                && parameters[0].ParameterType == typeof(CollectionQuery))
            {
                validateMethod = method;
                break;
            }
        }

        if (validateMethod is null)
        {
            Console.WriteLine("  Warning: Could not find Validate method on LagoonLinkValidator.");
            return;
        }

        // Validate is synchronous — no Task wrapping needed
        var result = validateMethod.Invoke(validator, [collectionQuery, configInstance]);
        await Task.CompletedTask;

        if (result is null)
        {
            return;
        }

        // Read result properties via dynamic
        dynamic validationResult = result;
        int pagesScanned;
        int linksChecked;
        int errorCount;
        try
        {
            pagesScanned = (int)validationResult.PagesScanned;
            linksChecked = (int)validationResult.LinksChecked;
            errorCount = (int)validationResult.Errors.Count;
        }
        catch
        {
            return;
        }

        if (errorCount == 0)
        {
            Console.WriteLine($"  Links:   {linksChecked} checked across {pagesScanned} page(s) — all valid");
            return;
        }

        Console.WriteLine($"  Links:   {errorCount} broken link(s) found across {pagesScanned} page(s):");
        try
        {
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine($"    ✗ {error.Message}");
            }
        }
        catch
        {
            // Ignore enumeration failures
        }

        Console.WriteLine();
        Console.WriteLine($"  Error: Link validation failed — {errorCount} broken link(s) detected.");
    }

    /// <summary>
    /// Writes core Atoll framework assets to the output directory:
    /// <c>_atoll/island.js</c>, <c>_atoll/directives.js</c>, and (if Atoll.Lagoon
    /// is referenced) <c>_atoll/logo.png</c>. These are embedded resources served
    /// dynamically by the dev server but must be written to disk for SSG output.
    /// </summary>
    private static async Task WriteCoreFrameworkAssetsAsync(Assembly? userAssembly, string outputDirectory)
    {
        var atollDir = Path.Combine(outputDirectory, "_atoll");
        Directory.CreateDirectory(atollDir);

        var count = 0;

        // island.js
        var islandJs = IslandScriptProvider.GetIslandScript();
        await File.WriteAllTextAsync(Path.Combine(atollDir, "island.js"), islandJs);
        count++;

        // directives.js
        var directivesJs = IslandScriptProvider.GetDirectivesScript();
        await File.WriteAllTextAsync(Path.Combine(atollDir, "directives.js"), directivesJs);
        count++;

        // logo.png — loaded via reflection to avoid a hard dependency on Atoll.Lagoon
        var logoPng = GetLagoonLogoPng(userAssembly);
        if (logoPng is not null)
        {
            await File.WriteAllBytesAsync(Path.Combine(atollDir, "logo.png"), logoPng);
            count++;
        }

        Console.WriteLine($"  Core:    {count} framework asset(s) written");
    }

    /// <summary>
    /// Loads the Atoll logo PNG from the <c>Atoll.Lagoon</c> assembly via reflection.
    /// Returns <c>null</c> if the assembly is not referenced or the resource is missing.
    /// </summary>
    private static byte[]? GetLagoonLogoPng(Assembly? userAssembly)
    {
        // Try to find Atoll.Lagoon in already-loaded assemblies first
        var lagoonAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Atoll.Lagoon");

        // If not found, try loading from the user assembly's output directory
        if (lagoonAssembly is null && userAssembly is not null)
        {
            var assemblyDir = Path.GetDirectoryName(userAssembly.Location);
            if (assemblyDir is not null)
            {
                var lagoonDll = Path.Combine(assemblyDir, "Atoll.Lagoon.dll");
                if (File.Exists(lagoonDll))
                {
                    try
                    {
                        lagoonAssembly = Assembly.LoadFrom(lagoonDll);
                    }
                    catch
                    {
                        // Ignore load failures
                    }
                }
            }
        }

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
        return ms.ToArray();
    }

    /// <summary>
    /// Generates OG images if the user's assembly implements
    /// <c>Atoll.Lagoon.OpenGraph.IOgImageConfiguration</c>.
    /// Uses reflection to avoid a hard dependency on <c>Atoll.Lagoon</c>.
    /// </summary>
    private static async Task GenerateOgImagesAsync(
        Assembly assembly,
        CollectionQuery collectionQuery,
        string outputDirectory,
        string projectRoot)
    {
        const string interfaceName = "Atoll.Lagoon.OpenGraph.IOgImageConfiguration";
        const string generatorTypeName = "Atoll.Lagoon.OpenGraph.OgImageGenerator";

        // Find an IOgImageConfiguration implementation in the user's assembly
        Type? configType = null;
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            var iface = type.GetInterface(interfaceName);
            if (iface is not null)
            {
                configType = type;
                break;
            }
        }

        if (configType is null)
        {
            return;
        }

        // Resolve OgImageGenerator from the assembly's dependencies
        Type? generatorType = null;
        foreach (var referencedAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            generatorType = referencedAssembly.GetType(generatorTypeName);
            if (generatorType is not null)
            {
                break;
            }
        }

        // Also try to find it via the assembly's output directory
        if (generatorType is null)
        {
            var assemblyDir = Path.GetDirectoryName(assembly.Location);
            if (assemblyDir is not null)
            {
                var lagoonDll = Path.Combine(assemblyDir, "Atoll.Lagoon.dll");
                if (File.Exists(lagoonDll))
                {
                    try
                    {
                        var lagoonAssembly = Assembly.LoadFrom(lagoonDll);
                        generatorType = lagoonAssembly.GetType(generatorTypeName);
                    }
                    catch
                    {
                        // Ignore load failures
                    }
                }
            }
        }

        if (generatorType is null)
        {
            Console.WriteLine("  Warning: IOgImageConfiguration found but Atoll.Lagoon assembly could not be loaded.");
            return;
        }

        // Create config instance
        var configInstance = Activator.CreateInstance(configType)!;

        // Create generator instance: new OgImageGenerator(outputDirectory, projectRoot)
        var generator = Activator.CreateInstance(generatorType, outputDirectory, projectRoot)!;

        // Find GenerateAsync(CollectionQuery, IOgImageConfiguration) — the 2-parameter overload
        MethodInfo? generateMethod = null;
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
                generateMethod = method;
                break;
            }
        }

        if (generateMethod is null)
        {
            Console.WriteLine("  Warning: Could not find GenerateAsync method on OgImageGenerator.");
            return;
        }

        var task = (Task)generateMethod.Invoke(generator, [collectionQuery, configInstance])!;
        await task;

        // Get result stats via dynamic to avoid reflection on Task<T>.Result
        dynamic taskResult = task;
        int imageCount;
        try
        {
            imageCount = (int)taskResult.Result.ImageCount;
        }
        catch
        {
            imageCount = 0;
        }

        Console.WriteLine($"  OG:     {imageCount} images generated");
    }

    /// <summary>
    /// Generates the search index if the user's assembly implements
    /// <c>Atoll.Lagoon.Search.ISearchIndexConfiguration</c>.
    /// Uses reflection to avoid a hard dependency on <c>Atoll.Lagoon</c>.
    /// </summary>
    private static async Task GenerateSearchIndexAsync(
        Assembly assembly,
        CollectionQuery collectionQuery,
        string outputDirectory)
    {
        const string interfaceName = "Atoll.Lagoon.Search.ISearchIndexConfiguration";
        const string generatorTypeName = "Atoll.Lagoon.Search.LagoonSearchIndexGenerator";

        // Find an ISearchIndexConfiguration implementation in the user's assembly
        Type? configType = null;
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            var iface = type.GetInterface(interfaceName);
            if (iface is not null)
            {
                configType = type;
                break;
            }
        }

        if (configType is null)
        {
            return;
        }

        // Resolve LagoonSearchIndexGenerator from the assembly's dependencies
        Type? generatorType = null;
        foreach (var referencedAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            generatorType = referencedAssembly.GetType(generatorTypeName);
            if (generatorType is not null)
            {
                break;
            }
        }

        // Also try to find it via the assembly load context
        if (generatorType is null)
        {
            var assemblyDir = Path.GetDirectoryName(assembly.Location);
            if (assemblyDir is not null)
            {
                var lagoonDll = Path.Combine(assemblyDir, "Atoll.Lagoon.dll");
                if (File.Exists(lagoonDll))
                {
                    try
                    {
                        var lagoonAssembly = Assembly.LoadFrom(lagoonDll);
                        generatorType = lagoonAssembly.GetType(generatorTypeName);
                    }
                    catch
                    {
                        // Ignore load failures
                    }
                }
            }
        }

        if (generatorType is null)
        {
            Console.WriteLine("  Warning: ISearchIndexConfiguration found but Atoll.Lagoon assembly could not be loaded.");
            return;
        }

        // Create config instance
        var configInstance = Activator.CreateInstance(configType)!;

        // Create generator instance: new LagoonSearchIndexGenerator(outputDirectory)
        var generator = Activator.CreateInstance(generatorType, outputDirectory)!;

        // Find GenerateAsync(CollectionQuery, ISearchIndexConfiguration) by scanning methods
        MethodInfo? generateMethod = null;
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
                generateMethod = method;
                break;
            }
        }

        if (generateMethod is null)
        {
            Console.WriteLine("  Warning: Could not find GenerateAsync method on LagoonSearchIndexGenerator.");
            return;
        }

        var task = (Task)generateMethod.Invoke(generator, [collectionQuery, configInstance])!;
        await task;

        // Get result stats via dynamic to avoid reflection on Task<T>.Result
        dynamic taskResult = task;
        int entryCount;
        try
        {
            entryCount = (int)taskResult.Result.EntryCount;
        }
        catch
        {
            entryCount = 0;
        }

        Console.WriteLine($"  Search:  {entryCount} entries indexed");
    }

    /// <summary>
    /// Generates the <c>_redirects</c> file if the user's assembly implements
    /// <c>Atoll.Lagoon.Redirects.IRedirectConfiguration</c>.
    /// Uses reflection to avoid a hard dependency on <c>Atoll.Lagoon</c>.
    /// </summary>
    private static async Task GenerateRedirectsAsync(
        Assembly assembly,
        CollectionQuery collectionQuery,
        string outputDirectory)
    {
        const string interfaceName = "Atoll.Lagoon.Redirects.IRedirectConfiguration";
        const string generatorTypeName = "Atoll.Lagoon.Redirects.LagoonRedirectGenerator";

        // Find an IRedirectConfiguration implementation in the user's assembly
        Type? configType = null;
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            var iface = type.GetInterface(interfaceName);
            if (iface is not null)
            {
                configType = type;
                break;
            }
        }

        if (configType is null)
        {
            return;
        }

        // Resolve LagoonRedirectGenerator from the assembly's dependencies
        Type? generatorType = null;
        foreach (var referencedAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            generatorType = referencedAssembly.GetType(generatorTypeName);
            if (generatorType is not null)
            {
                break;
            }
        }

        // Also try to find it via the assembly directory
        if (generatorType is null)
        {
            var assemblyDir = Path.GetDirectoryName(assembly.Location);
            if (assemblyDir is not null)
            {
                var lagoonDll = Path.Combine(assemblyDir, "Atoll.Lagoon.dll");
                if (File.Exists(lagoonDll))
                {
                    try
                    {
                        var lagoonAssembly = Assembly.LoadFrom(lagoonDll);
                        generatorType = lagoonAssembly.GetType(generatorTypeName);
                    }
                    catch
                    {
                        // Ignore load failures
                    }
                }
            }
        }

        if (generatorType is null)
        {
            Console.WriteLine("  Warning: IRedirectConfiguration found but Atoll.Lagoon assembly could not be loaded.");
            return;
        }

        // Create config instance
        var configInstance = Activator.CreateInstance(configType)!;

        // Create generator instance: new LagoonRedirectGenerator(outputDirectory)
        var generator = Activator.CreateInstance(generatorType, outputDirectory)!;

        // Find GenerateAsync(CollectionQuery, IRedirectConfiguration) by scanning methods
        MethodInfo? generateMethod = null;
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
                generateMethod = method;
                break;
            }
        }

        if (generateMethod is null)
        {
            Console.WriteLine("  Warning: Could not find GenerateAsync method on LagoonRedirectGenerator.");
            return;
        }

        var task = (Task)generateMethod.Invoke(generator, [collectionQuery, configInstance])!;
        await task;

        // Get result stats via dynamic to avoid reflection on Task<T>.Result
        dynamic taskResult = task;
        int ruleCount;
        try
        {
            ruleCount = (int)taskResult.Result.RuleCount;
        }
        catch
        {
            ruleCount = 0;
        }

        Console.WriteLine($"  Redirects: {ruleCount} rule(s) written to _redirects");
    }

    /// <summary>
    /// Discovers routes by scanning the assembly for types that implement
    /// <see cref="IAtollPage"/>. Uses the <see cref="PageRouteAttribute"/> if present,
    /// otherwise infers the route pattern from the type name.
    /// </summary>
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

            // Check for explicit [PageRoute] attribute first
            var routeAttr = type.GetCustomAttribute<PageRouteAttribute>();
            if (routeAttr is not null)
            {
                var pattern = routeAttr.Pattern.StartsWith('/')
                    ? routeAttr.Pattern
                    : "/" + routeAttr.Pattern;
                entries.Add(new RouteEntry(pattern, type, pattern));
                continue;
            }

            // Fall back to convention-based inference
            var filePath = InferFilePathFromType(type);
            var inferredEntries = RouteDiscovery.DiscoverRoutesFromEntries(
                new[] { (filePath, type) });
            entries.AddRange(inferredEntries);
        }

        return entries;
    }

    /// <summary>
    /// Infers a route file path from a page type name using Atoll conventions.
    /// </summary>
    private static string InferFilePathFromType(Type type)
    {
        var name = type.Name;

        // Strip "Page" suffix if present
        if (name.EndsWith("Page", StringComparison.Ordinal))
        {
            name = name[..^4];
        }

        // Convert PascalCase to kebab-case path segments
        // e.g., "BlogPost" -> "blog-post", "TagsIndex" -> "tags-index"
        // Special case: "Index" -> "index" (root of directory)
        if (name.Equals("Index", StringComparison.OrdinalIgnoreCase))
        {
            return "index.cs";
        }

        // Check if this is an index page for a subdirectory
        // e.g., "BlogIndex" -> "blog/index.cs", "TagsIndex" -> "tags/index.cs"
        if (name.EndsWith("Index", StringComparison.Ordinal))
        {
            var prefix = ToKebabCase(name[..^5]);
            return prefix + "/index.cs";
        }

        // Check for dynamic route types (implement IStaticPathsProvider)
        // e.g., "BlogPost" -> "blog/[slug].cs", "Tag" -> "tags/[tag].cs"
        if (typeof(IStaticPathsProvider).IsAssignableFrom(type))
        {
            var segment = ToKebabCase(name);
            // For dynamic routes, use the parent directory from namespace or type name
            // and a [slug] parameter. The actual parameter name comes from GetStaticPaths.
            return segment + "/[slug].cs";
        }

        // Static page: "About" -> "about.cs"
        return ToKebabCase(name) + ".cs";
    }

    /// <summary>
    /// Attempts to build a <see cref="RedirectMap"/> from the loaded user assembly.
    /// If the assembly's types include an <c>IRedirectMapProvider</c> implementation,
    /// its <c>GetRedirectMap()</c> method is called via reflection. Otherwise,
    /// <see cref="RedirectMap.Empty"/> is returned so that a stable (empty)
    /// <c>redirects.json</c> is always written to the output directory.
    /// </summary>
    private static RedirectMap BuildRedirectMapFromAssembly(Assembly? assembly)
    {
        if (assembly is null)
        {
            return RedirectMap.Empty;
        }

        const string interfaceName = "Atoll.Redirects.IRedirectMapProvider";

        foreach (var type in assembly.GetExportedTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            var iface = type.GetInterface(interfaceName);
            if (iface is null)
            {
                continue;
            }

            try
            {
                var instance = Activator.CreateInstance(type)!;
                var method = type.GetMethod("GetRedirectMap");
                if (method is not null)
                {
                    var result = method.Invoke(instance, null);
                    if (result is RedirectMap map)
                    {
                        return map;
                    }
                }
            }
            catch
            {
                // If reflection-based loading fails, fall back to empty map
            }

            break;
        }

        return RedirectMap.Empty;
    }

    /// <summary>
    /// Converts a PascalCase name to kebab-case.
    /// </summary>
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

    /// <summary>
    /// Finds the .csproj file in the project root directory.
    /// </summary>
    private static string? FindProjectFile(string projectRoot)
    {
        var csprojFiles = Directory.GetFiles(projectRoot, "*.csproj", SearchOption.TopDirectoryOnly);
        return csprojFiles.Length switch
        {
            0 => null,
            1 => csprojFiles[0],
            _ => csprojFiles[0], // Use first if multiple
        };
    }

    /// <summary>
    /// Builds the project using <c>dotnet build</c>.
    /// </summary>
    private static async Task<bool> BuildProjectAsync(string csprojPath)
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
            return false;
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            if (error.Length > 0)
            {
                Console.WriteLine($"  Build errors:\n{error}");
            }
        }

        return process.ExitCode == 0;
    }

    /// <summary>
    /// Finds the output assembly DLL for the project.
    /// </summary>
    private static string? FindOutputAssembly(string csprojPath)
    {
        var projectDir = Path.GetDirectoryName(csprojPath)!;
        var projectName = Path.GetFileNameWithoutExtension(csprojPath);

        // Check Release first, then Debug
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

            // Find the TFM directory (e.g., net10.0)
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
    /// Loads an assembly from the specified path using an isolated load context.
    /// </summary>
    private static Assembly? LoadAssembly(string assemblyPath)
    {
        try
        {
            var loadContext = new AssemblyLoadContext("AtollBuild", isCollectible: false);

            // Resolve transitive NuGet dependencies (e.g. TextMateSharp) by
            // parsing the user project's .deps.json and probing the NuGet cache.
            // AssemblyDependencyResolver does not work for class library projects
            // (no .runtimeconfig.json), so we use our own resolver.
            var resolver = DepsJsonAssemblyResolver.Create(assemblyPath);
            resolver?.Attach(loadContext);

            return loadContext.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Failed to load assembly '{assemblyPath}': {ex.Message}");
            return null;
        }
    }
}

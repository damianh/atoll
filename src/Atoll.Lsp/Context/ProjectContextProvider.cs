using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Microsoft.Extensions.Logging;

namespace Atoll.Lsp.Context;

/// <summary>
/// Discovers the user's project context by scanning for a <see cref="IContentConfiguration"/>
/// implementation in the compiled project assembly. Operates in the background and exposes
/// the current context as a nullable property, falling back gracefully when unavailable.
/// </summary>
internal sealed class ProjectContextProvider : IDisposable
{
    private readonly ILogger<ProjectContextProvider> _logger;
    private volatile ProjectContext? _context;
    private string? _workspaceRoot;
    private FileSystemWatcher? _watcher;
    private Timer? _reloadDebounce;
    private const int ReloadDebounceMs = 800;

    internal ProjectContextProvider(ILogger<ProjectContextProvider> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// The current project context, or <c>null</c> if no assembly has been successfully loaded.
    /// </summary>
    internal ProjectContext? Current => _context;

    /// <summary>
    /// Fired when the context is updated (assembly reloaded).
    /// </summary>
    internal event Action<ProjectContext?>? ContextChanged;

    /// <summary>
    /// Initialises context discovery from the given workspace root path.
    /// </summary>
    internal async Task InitialiseAsync(string workspaceRoot, CancellationToken cancellationToken = default)
    {
        _workspaceRoot = workspaceRoot;
        _logger.LogInformation("Initialising project context from workspace: {Root}", workspaceRoot);

        var loaded = await TryLoadContextAsync(workspaceRoot, cancellationToken).ConfigureAwait(false);
        if (loaded is not null)
        {
            SetContext(loaded);
            WatchForRebuild(loaded);
        }
        else
        {
            _logger.LogWarning(
                "No Atoll project assembly found. " +
                "Build your project to enable component-aware diagnostics.");
        }
    }

    private void SetContext(ProjectContext context)
    {
        _context = context;
        ContextChanged?.Invoke(context);
    }

    private async Task<ProjectContext?> TryLoadContextAsync(
        string workspaceRoot,
        CancellationToken cancellationToken)
    {
        try
        {
            var dllPath = FindProjectDll(workspaceRoot);
            if (dllPath is null)
            {
                _logger.LogDebug("Could not locate project DLL under {Root}", workspaceRoot);
                return null;
            }

            _logger.LogInformation("Loading project assembly from {Dll}", dllPath);
            return await Task.Run(() => LoadFromAssembly(dllPath, workspaceRoot), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load project context");
            return null;
        }
    }

    private static string? FindProjectDll(string workspaceRoot)
    {
        // Find *.csproj files (excluding test projects)
        var csprojFiles = Directory
            .EnumerateFiles(workspaceRoot, "*.csproj", SearchOption.TopDirectoryOnly)
            .Where(f => !Path.GetFileName(f).Contains(".Tests.", StringComparison.OrdinalIgnoreCase) &&
                        !Path.GetFileName(f).Contains(".Test.", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (csprojFiles.Count == 0)
        {
            // Try one level deeper
            csprojFiles = Directory
                .EnumerateFiles(workspaceRoot, "*.csproj", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).Contains(".Tests.", StringComparison.OrdinalIgnoreCase) &&
                            !Path.GetFileName(f).Contains(".Test.", StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();
        }

        foreach (var csprojPath in csprojFiles)
        {
            var dll = FindDllForCsproj(csprojPath);
            if (dll is not null)
            {
                return dll;
            }
        }

        return null;
    }

    private static string? FindDllForCsproj(string csprojPath)
    {
        try
        {
            var xdoc = XDocument.Load(csprojPath);
            var assemblyName = xdoc.Descendants("AssemblyName").FirstOrDefault()?.Value
                               ?? Path.GetFileNameWithoutExtension(csprojPath);

            var targetFramework = xdoc.Descendants("TargetFramework").FirstOrDefault()?.Value
                                  ?? "net10.0";

            var projDir = Path.GetDirectoryName(csprojPath)!;

            // Check Debug/Release
            foreach (var config in new[] { "Debug", "Release" })
            {
                var candidate = Path.Combine(projDir, "bin", config, targetFramework, $"{assemblyName}.dll");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private ProjectContext? LoadFromAssembly(string dllPath, string workspaceRoot)
    {
        var alc = new AssemblyLoadContext($"AtollLsp-{Path.GetFileName(dllPath)}", isCollectible: true);
        try
        {
            // Also resolve dependencies from the same directory
            var dllDir = Path.GetDirectoryName(dllPath)!;
            alc.Resolving += (_, assemblyName) =>
            {
                var candidate = Path.Combine(dllDir, $"{assemblyName.Name}.dll");
                return File.Exists(candidate) ? alc.LoadFromAssemblyPath(candidate) : null;
            };

            var assembly = alc.LoadFromAssemblyPath(dllPath);
            return ExtractContext(assembly, workspaceRoot);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract context from assembly {Dll}", dllPath);
            alc.Unload();
            return null;
        }
    }

    private static ProjectContext? ExtractContext(Assembly assembly, string workspaceRoot)
    {
        // Find IContentConfiguration implementation
        Type? configType = null;
        foreach (var type in assembly.GetExportedTypes())
        {
            if (typeof(IContentConfiguration).IsAssignableFrom(type) &&
                type is { IsAbstract: false, IsInterface: false })
            {
                configType = type;
                break;
            }
        }

        if (configType is null)
        {
            return null;
        }

        var configInstance = (IContentConfiguration)Activator.CreateInstance(configType)!;
        var collectionConfig = configInstance.Configure();

        var components = BuildComponentDictionary(collectionConfig.Markdown?.Components);
        var collections = BuildCollectionDictionary(collectionConfig, workspaceRoot);

        return new ProjectContext(components, collections, collectionConfig.BaseDirectory);
    }

    private static Dictionary<string, ComponentInfo> BuildComponentDictionary(ComponentMap? componentMap)
    {
        var result = new Dictionary<string, ComponentInfo>(StringComparer.OrdinalIgnoreCase);
        if (componentMap is null)
        {
            return result;
        }

        foreach (var name in componentMap.RegisteredNames)
        {
            if (!componentMap.TryResolve(name, out var type) || type is null)
            {
                continue;
            }

            var info = BuildComponentInfo(name, type);
            result[name] = info;

            // Also add PascalCase alias if different
            var typeName = type.Name;
            if (!result.ContainsKey(typeName))
            {
                result[typeName] = info;
            }
        }

        return result;
    }

    private static ComponentInfo BuildComponentInfo(string name, Type type)
    {
        var parameters = new List<ComponentParameterInfo>();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var paramAttr = prop.GetCustomAttribute<ParameterAttribute>();
            if (paramAttr is not null)
            {
                parameters.Add(new ComponentParameterInfo(prop.Name, prop.PropertyType, paramAttr.Required));
            }
        }

        return new ComponentInfo(name, type.Name, type.FullName, parameters);
    }

    private static Dictionary<string, CollectionSchemaInfo> BuildCollectionDictionary(
        CollectionConfig config,
        string workspaceRoot)
    {
        var result = new Dictionary<string, CollectionSchemaInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, collection) in config.Collections)
        {
            var directory = Path.Combine(config.BaseDirectory, name)
                .Replace('\\', '/');

            var properties = BuildSchemaProperties(collection.SchemaType);
            result[name] = new CollectionSchemaInfo(name, directory, properties);
        }

        return result;
    }

    private static List<SchemaPropertyInfo> BuildSchemaProperties(Type schemaType)
    {
        var result = new List<SchemaPropertyInfo>();
        foreach (var prop in schemaType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var required = prop.GetCustomAttribute<RequiredAttribute>() is not null;
            var yamlKey = ToCamelCase(prop.Name);
            result.Add(new SchemaPropertyInfo(prop.Name, yamlKey, prop.PropertyType, required));
        }

        return result;
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private void WatchForRebuild(ProjectContext context)
    {
        // Find the bin directories of all collections' source projects
        if (_workspaceRoot is null)
        {
            return;
        }

        var csprojFiles = Directory.EnumerateFiles(_workspaceRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Contains(".Tests.", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var csproj in csprojFiles)
        {
            var projDir = Path.GetDirectoryName(csproj);
            if (projDir is null)
            {
                continue;
            }

            var binDir = Path.Combine(projDir, "bin");
            if (!Directory.Exists(binDir))
            {
                continue;
            }

            var watcher = new FileSystemWatcher(binDir, "*.dll")
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };
            watcher.Changed += OnDllChanged;
            _watcher = watcher; // store last watcher (simplified — could track multiple)
            break; // Watch the first project's bin
        }
    }

    private void OnDllChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce rapid file system events during build
        _reloadDebounce?.Dispose();
        _reloadDebounce = new Timer(
            _ => ReloadContextFireAndForget(),
            null,
            ReloadDebounceMs,
            Timeout.Infinite);
    }

    private void ReloadContextFireAndForget()
    {
        if (_workspaceRoot is null)
        {
            return;
        }

        var root = _workspaceRoot;
        _logger.LogInformation("DLL changed, reloading project context");

        _ = Task.Run(async () =>
        {
            try
            {
                var newContext = await TryLoadContextAsync(root, CancellationToken.None)
                    .ConfigureAwait(false);
                if (newContext is not null)
                {
                    SetContext(newContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to reload project context after DLL change");
            }
        });
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _reloadDebounce?.Dispose();
    }
}

using System.Reflection;
using System.Runtime.Loader;
using Atoll.Content.Collections;
using Atoll.Core.Configuration;
using Atoll.Middleware.Server.Hosting;
using Atoll.Routing;
using Atoll.Routing.FileSystem;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Atoll.Cli.Commands;

/// <summary>
/// Handles the <c>atoll dev</c> command. Starts the ASP.NET Core development server
/// with Atoll middleware for live rendering of pages and endpoints.
/// </summary>
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

        // Build the user project first
        var csprojPath = FindProjectFile(projectRoot);
        Assembly? userAssembly = null;

        if (csprojPath is not null)
        {
            Console.WriteLine($"  Building {Path.GetFileName(csprojPath)}...");
            var buildSuccess = await BuildProjectAsync(csprojPath);
            if (buildSuccess)
            {
                var assemblyPath = FindOutputAssembly(csprojPath);
                if (assemblyPath is not null)
                {
                    userAssembly = LoadAssembly(assemblyPath);
                }
            }
            else
            {
                Console.WriteLine("  Warning: Build failed — starting dev server with no routes.");
            }
        }

        // Discover content configuration for service props
        var serviceProps = BuildServiceProps(userAssembly, projectRoot);

        // Start the dev server
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://{config.Server.Host}:{effectivePort}");

        builder.Services.AddAtoll(options =>
        {
            if (userAssembly is not null)
            {
                options.Assemblies.Add(userAssembly);
            }

            foreach (var kvp in serviceProps)
            {
                options.ServiceProps[kvp.Key] = kvp.Value;
            }
        });

        var app = builder.Build();
        app.UseAtoll();

        Console.WriteLine($"Atoll — dev server starting on http://{config.Server.Host}:{effectivePort}");
        Console.WriteLine("  Press Ctrl+C to stop.");

        await app.RunAsync();
    }

    /// <summary>
    /// Builds service props by discovering content configuration from the loaded assembly.
    /// </summary>
    private static Dictionary<string, object?> BuildServiceProps(
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
        return new CollectionQuery(loader);
    }

    /// <summary>
    /// Finds the .csproj file in the project root directory.
    /// </summary>
    private static string? FindProjectFile(string projectRoot)
    {
        var csprojFiles = Directory.GetFiles(projectRoot, "*.csproj", SearchOption.TopDirectoryOnly);
        return csprojFiles.Length > 0 ? csprojFiles[0] : null;
    }

    /// <summary>
    /// Builds the project using <c>dotnet build</c>.
    /// </summary>
    private static async Task<bool> BuildProjectAsync(string csprojPath)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{csprojPath}\" -c Release --nologo -v q",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = System.Diagnostics.Process.Start(psi);
        if (process is null)
        {
            return false;
        }

        await process.WaitForExitAsync();
        return process.ExitCode == 0;
    }

    /// <summary>
    /// Finds the output assembly DLL for the project.
    /// </summary>
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
    /// Loads an assembly from the specified path using an isolated load context.
    /// </summary>
    private static Assembly? LoadAssembly(string assemblyPath)
    {
        try
        {
            var loadContext = new AssemblyLoadContext("AtollDev", isCollectible: false);
            loadContext.Resolving += (context, assemblyName) =>
            {
                var dir = Path.GetDirectoryName(assemblyPath)!;
                var candidate = Path.Combine(dir, assemblyName.Name + ".dll");
                return File.Exists(candidate) ? context.LoadFromAssemblyPath(candidate) : null;
            };

            return loadContext.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Failed to load assembly: {ex.Message}");
            return null;
        }
    }
}

using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Loader;
using Atoll.Middleware.Server.Hosting;
using Atoll.Routing.Matching;

namespace Atoll.Cli.Commands.Dev;

/// <summary>
/// An immutable snapshot of all state required to serve HTTP requests during
/// <c>atoll dev</c>. Swapped atomically on hot-reload.
/// </summary>
/// <param name="RouteMatcher">The route matcher built from the current route table.</param>
/// <param name="Options">The Atoll options including base path and service props.</param>
/// <param name="LoadContext">
/// The <see cref="AssemblyLoadContext"/> that owns the user assembly, or <c>null</c>
/// if no user assembly is loaded. Retained so the previous ALC can be scheduled for
/// unload after a code-change reload.
/// </param>
/// <param name="UserAssembly">
/// The user project assembly, or <c>null</c> if the build failed or no project was found.
/// </param>
/// <param name="GlobalCss">
/// Pre-aggregated global CSS discovered from <c>[GlobalStyle]</c> components in the user
/// assembly and its referenced assemblies. Injected as an inline <c>&lt;style&gt;</c> tag
/// into every page response. Empty string when no global styles are found.
/// </param>
/// <param name="IslandAssets">
/// In-memory map of island JavaScript assets discovered from <see cref="Atoll.Islands.IIslandAssetProvider"/>
/// implementations. Keys are URL paths (e.g. <c>scripts/atoll-theme-toggle.js</c>), values
/// are the file content bytes read from embedded resources. Served directly by
/// <see cref="DevAtollRequestHandler"/> when the browser requests the corresponding URL.
/// </param>
/// <param name="SearchIndexJson">
/// Pre-generated search index JSON bytes, or <c>null</c> when no
/// <c>ISearchIndexConfiguration</c> is found. Served at <c>/search-index.json</c>.
/// </param>
/// <param name="ShadowCopyDir">
/// Temporary directory containing the shadow-copied assembly files, or <c>null</c> when
/// no assembly is loaded. Deleted after the <see cref="LoadContext"/> is unloaded to
/// reclaim disk space.
/// </param>
internal sealed record DevServerState(
    RouteMatcher RouteMatcher,
    AtollOptions Options,
    AssemblyLoadContext? LoadContext,
    Assembly? UserAssembly,
    string GlobalCss,
    IReadOnlyDictionary<string, byte[]> IslandAssets,
    byte[]? SearchIndexJson,
    string? ShadowCopyDir)
{
    private static readonly IReadOnlyDictionary<string, byte[]> EmptyAssets =
        new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>());

    /// <summary>
    /// Gets an empty state with no routes and no loaded assembly.
    /// Used when no project file is present or the initial build fails.
    /// </summary>
    public static DevServerState Empty { get; } =
        new DevServerState(new RouteMatcher([]), new AtollOptions(), null, null, "", EmptyAssets, null, null);
}

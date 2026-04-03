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
internal sealed record DevServerState(
    RouteMatcher RouteMatcher,
    AtollOptions Options,
    AssemblyLoadContext? LoadContext,
    Assembly? UserAssembly)
{
    /// <summary>
    /// Gets an empty state with no routes and no loaded assembly.
    /// Used when no project file is present or the initial build fails.
    /// </summary>
    public static DevServerState Empty { get; } =
        new DevServerState(new RouteMatcher([]), new AtollOptions(), null, null);
}

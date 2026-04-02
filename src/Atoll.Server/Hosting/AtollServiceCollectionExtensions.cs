using Atoll.Routing;
using Atoll.Routing.FileSystem;
using Atoll.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atoll.Server.Hosting;

/// <summary>
/// Extension methods for registering Atoll services in the ASP.NET Core
/// dependency injection container.
/// </summary>
public static class AtollServiceCollectionExtensions
{
    /// <summary>
    /// Adds Atoll services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">
    /// A delegate that configures the <see cref="AtollOptions"/> for the application.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddAtoll(
        this IServiceCollection services,
        Action<AtollOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new AtollOptions();
        configure(options);

        // Build route entries from either explicit entries or assembly scanning
        IReadOnlyList<RouteEntry> routes;

        if (options.RouteEntries.Count > 0)
        {
            routes = RouteDiscovery.DiscoverRoutesFromEntries(options.RouteEntries);
        }
        else if (options.Assemblies.Count > 0)
        {
            // Use a dummy pages directory for assembly-based scanning
            // (routes are discovered from type metadata, not file system)
            var discovery = new RouteDiscovery("src/pages");
            routes = discovery.DiscoverRoutes(options.Assemblies);
        }
        else
        {
            routes = [];
        }

        var routeMatcher = new RouteMatcher(routes);

        services.AddSingleton(options);
        services.AddSingleton(routeMatcher);
        services.AddSingleton<AtollRequestHandler>();

        return services;
    }
}

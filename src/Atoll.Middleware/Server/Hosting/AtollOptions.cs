using System.Reflection;
using Atoll.Build.Content.Collections;

namespace Atoll.Middleware.Server.Hosting;

/// <summary>
/// Configuration options for the Atoll server middleware.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AtollOptions"/> controls how the Atoll middleware discovers routes,
/// resolves component types, and renders pages. It is configured via
/// <see cref="AtollServiceCollectionExtensions.AddAtoll"/> during application startup.
/// </para>
/// </remarks>
public sealed class AtollOptions
{
    /// <summary>
    /// Gets or sets the base path prefix for all Atoll routes.
    /// When set (e.g., <c>/docs</c>), Atoll only handles requests whose path starts
    /// with this prefix, and the prefix is stripped before route matching.
    /// Defaults to <c>/</c> (root).
    /// </summary>
    public string BasePath { get; set; } = "/";

    /// <summary>
    /// Gets or sets the site URL used for generating absolute URLs.
    /// </summary>
    public Uri? SiteUrl { get; set; }

    /// <summary>
    /// Gets the list of assemblies to scan for routable types
    /// (<see cref="Atoll.Routing.IAtollPage"/> and <see cref="Atoll.Routing.IAtollEndpoint"/>).
    /// </summary>
    public IList<Assembly> Assemblies { get; } = new List<Assembly>();

    /// <summary>
    /// Gets the explicit route entry mappings. When provided, these take precedence
    /// over assembly scanning and file-based discovery.
    /// </summary>
    /// <remarks>
    /// Each entry maps a relative file path (e.g., <c>blog/[slug].cs</c>) to
    /// the component type that handles that route.
    /// </remarks>
    public IList<(string RelativeFilePath, Type ComponentType)> RouteEntries { get; } =
        new List<(string RelativeFilePath, Type ComponentType)>();

    /// <summary>
    /// Gets the service props dictionary. Service props are automatically merged into
    /// the props for every page render, providing shared dependencies like
    /// <see cref="CollectionQuery"/>.
    /// </summary>
    /// <remarks>
    /// Keys are matched case-insensitively to <c>[Parameter]</c>-marked properties
    /// on page components. Route-specific parameters take precedence over service props.
    /// </remarks>
    public IDictionary<string, object?> ServiceProps { get; } =
        new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
}

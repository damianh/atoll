using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Atoll.Hosting.Aspire;

/// <summary>
/// Extension methods for adding Atoll site resources to an Aspire distributed application.
/// </summary>
public static class AtollSiteResourceBuilderExtensions
{
    private const int DefaultPort = 4321;
    private const string HealthCheckPath = "/__health";

    /// <summary>
    /// Adds an Atoll site resource to the distributed application, launching the
    /// <c>atoll dev</c> command with live-reload and an HTTP health check endpoint.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="siteDirectory">The path to the Atoll site directory.</param>
    /// <returns>A resource builder for the Atoll site resource.</returns>
    public static IResourceBuilder<AtollSiteResource> AddAtollSite(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        string siteDirectory)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(siteDirectory);

        var absolutePath = Path.GetFullPath(siteDirectory);
        var resource = new AtollSiteResource(name, "atoll", absolutePath, absolutePath);

        return builder.AddResource(resource)
            .WithHttpEndpoint(targetPort: DefaultPort, name: "http")
            .WithHttpHealthCheck(endpointName: "http", path: HealthCheckPath)
            .WithArgs(context =>
            {
                context.Args.Add("dev");
                context.Args.Add("--root");
                context.Args.Add(resource.SiteDirectory);

                var endpoint = resource.GetEndpoint("http");
                context.Args.Add("--port");
                context.Args.Add(endpoint.TargetPort?.ToString() ?? DefaultPort.ToString());

                if (resource.WriteDist)
                {
                    context.Args.Add("--write-dist");
                }
            });
    }

    /// <summary>
    /// Configures the Atoll site to write rendered output to the <c>dist/</c>
    /// directory after each rebuild cycle.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IResourceBuilder<AtollSiteResource> WithWriteDist(
        this IResourceBuilder<AtollSiteResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Resource.WriteDist = true;
        return builder;
    }
}

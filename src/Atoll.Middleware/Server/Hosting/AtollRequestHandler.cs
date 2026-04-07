using System.Collections.ObjectModel;
using System.Text;
using Atoll.Build.Pipeline;
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using Atoll.Routing.Matching;

namespace Atoll.Middleware.Server.Hosting;

/// <summary>
/// Handles incoming HTTP requests by matching them against the Atoll route table
/// and dispatching to the appropriate page or endpoint component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AtollRequestHandler"/> is the core dispatcher that sits behind the
/// ASP.NET Core middleware. For each request it:
/// </para>
/// <list type="number">
/// <item>Strips the base path prefix (if configured)</item>
/// <item>Matches the request path against the <see cref="RouteMatcher"/></item>
/// <item>Determines whether the matched type is a page or endpoint</item>
/// <item>Renders the page (with layout wrapping) or dispatches the endpoint</item>
/// <item>Writes the result to the <see cref="HttpResponse"/></item>
/// </list>
/// </remarks>
public sealed class AtollRequestHandler
{
    private readonly RouteMatcher _routeMatcher;
    private readonly AtollOptions _options;
    private readonly ILogger<AtollRequestHandler> _logger;

    /// <summary>
    /// Initializes a new <see cref="AtollRequestHandler"/> with the specified route matcher,
    /// options, and logger.
    /// </summary>
    /// <param name="routeMatcher">The route matcher to use for URL matching.</param>
    /// <param name="options">The Atoll configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public AtollRequestHandler(
        RouteMatcher routeMatcher,
        AtollOptions options,
        ILogger<AtollRequestHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(routeMatcher);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _routeMatcher = routeMatcher;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Attempts to handle the incoming HTTP request. Returns <c>true</c> if the request
    /// was handled (a matching route was found); <c>false</c> if no route matched.
    /// </summary>
    /// <param name="context">The ASP.NET Core HTTP context.</param>
    /// <returns>
    /// A task that resolves to <c>true</c> if the request was handled; <c>false</c> otherwise.
    /// </returns>
    public async Task<bool> TryHandleAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var requestPath = ExtractRoutePath(context.Request.Path);
        if (requestPath is null)
        {
            return false;
        }

        var match = _routeMatcher.Match(requestPath);
        if (match is null)
        {
            _logger.LogDebug("No route matched for path '{Path}'", requestPath);
            return false;
        }

        _logger.LogDebug(
            "Route matched: '{Path}' -> {ComponentType} (pattern: {Pattern})",
            requestPath,
            match.RouteEntry.ComponentType.Name,
            match.RouteEntry.Pattern);

        if (typeof(IAtollEndpoint).IsAssignableFrom(match.RouteEntry.ComponentType))
        {
            await HandleEndpointAsync(context, match);
        }
        else if (typeof(IAtollComponent).IsAssignableFrom(match.RouteEntry.ComponentType))
        {
            await HandlePageAsync(context, match);
        }
        else
        {
            _logger.LogWarning(
                "Matched type '{Type}' is neither IAtollPage/IAtollComponent nor IAtollEndpoint",
                match.RouteEntry.ComponentType.FullName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Extracts the route path from the request path by stripping the base path prefix.
    /// Returns <c>null</c> if the request path does not start with the base path.
    /// </summary>
    private string? ExtractRoutePath(PathString requestPath)
    {
        var basePath = _options.BasePath.TrimEnd('/');
        var path = requestPath.Value ?? "/";

        if (basePath.Length > 1)
        {
            if (!path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            path = path[basePath.Length..];
            if (path.Length == 0)
            {
                path = "/";
            }
        }

        return path;
    }

    /// <summary>
    /// Handles an endpoint request by dispatching to the appropriate HTTP method handler.
    /// </summary>
    private async Task HandleEndpointAsync(HttpContext httpContext, RouteMatchResult match)
    {
        var endpoint = (IAtollEndpoint)Activator.CreateInstance(match.RouteEntry.ComponentType)!;

        var endpointRequest = BuildEndpointRequest(httpContext);
        var endpointContext = new EndpointContext(match.Parameters, endpointRequest);

        var response = await EndpointDispatcher.DispatchAsync(endpoint, endpointContext);

        await WriteAtollResponseAsync(httpContext.Response, response);
    }

    /// <summary>
    /// Handles a page request by rendering the page component (with layouts) and
    /// writing the HTML response, with optional ETag/304 conditional request support.
    /// </summary>
    private async Task HandlePageAsync(HttpContext httpContext, RouteMatchResult match)
    {
        var componentType = match.RouteEntry.ComponentType;

        // Build props from service props + route parameters
        var props = BuildPageProps(match.Parameters);

        // Render through PageRenderer for DOCTYPE/head injection
        var renderer = new PageRenderer();

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

            // Allow the page component to signal a non-200 HTTP status code
            if (component is IPageStatusCodeProvider statusProvider)
            {
                renderer.StatusCode = statusProvider.ResponseStatusCode;
            }
        };

        var result = await renderer.RenderPageAsync(renderDelegate);

        if (_options.EnableCacheControl)
        {
            // Compute ETag from rendered HTML bytes (full SHA-256, weak ETag)
            var bodyBytes = Encoding.UTF8.GetBytes(result.Html);
            var hash = AssetFingerprinter.ComputeHash(bodyBytes, 64);
            var etag = $"W/\"{hash}\"";

            // Check If-None-Match for 304 conditional response (RFC 7232 §3.2)
            var ifNoneMatch = httpContext.Request.Headers["If-None-Match"].ToString();
            if (ifNoneMatch == "*" || ifNoneMatch == etag)
            {
                httpContext.Response.StatusCode = 304;
                httpContext.Response.Headers["ETag"] = etag;
                return;
            }

            // Response with ETag and Cache-Control
            httpContext.Response.StatusCode = result.StatusCode;
            httpContext.Response.ContentType = "text/html; charset=utf-8";
            httpContext.Response.Headers["ETag"] = etag;
            httpContext.Response.Headers["Cache-Control"] = "no-cache";
            await httpContext.Response.Body.WriteAsync(bodyBytes);
        }
        else
        {
            httpContext.Response.StatusCode = result.StatusCode;
            httpContext.Response.ContentType = "text/html; charset=utf-8";
            await result.WriteToStreamAsync(httpContext.Response.Body);
        }
    }

    /// <summary>
    /// Builds an <see cref="EndpointRequest"/> from the ASP.NET Core <see cref="HttpRequest"/>.
    /// </summary>
    private static EndpointRequest BuildEndpointRequest(HttpContext httpContext)
    {
        var request = httpContext.Request;

        var url = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}");

        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            // Take the first value for each header (simplified for Atoll's abstraction)
            headers[header.Key] = header.Value.ToString();
        }

        return new EndpointRequest(
            request.Method,
            url,
            new ReadOnlyDictionary<string, string>(headers),
            request.Body);
    }

    /// <summary>
    /// Builds a props dictionary by merging service props (lowest priority)
    /// with route parameters (highest priority). Route parameters are boxed
    /// as <c>object?</c> values.
    /// </summary>
    private IReadOnlyDictionary<string, object?> BuildPageProps(
        IReadOnlyDictionary<string, string> parameters)
    {
        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        // Service props first (lowest priority)
        foreach (var kvp in _options.ServiceProps)
        {
            props[kvp.Key] = kvp.Value;
        }

        // Route parameters override service props
        foreach (var kvp in parameters)
        {
            props[kvp.Key] = kvp.Value;
        }

        return new ReadOnlyDictionary<string, object?>(props);
    }

    /// <summary>
    /// Writes an <see cref="AtollResponse"/> to the ASP.NET Core <see cref="HttpResponse"/>.
    /// </summary>
    private static async Task WriteAtollResponseAsync(HttpResponse httpResponse, AtollResponse atollResponse)
    {
        httpResponse.StatusCode = atollResponse.StatusCode;

        foreach (var header in atollResponse.Headers)
        {
            httpResponse.Headers[header.Key] = header.Value;
        }

        if (atollResponse.Body is not null)
        {
            await httpResponse.Body.WriteAsync(atollResponse.Body);
        }
    }
}
